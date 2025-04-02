using System.Collections.Concurrent;
using AbyssIrc.Network.Data.Channels;

namespace AbyssIrc.Server.Core.Data.Sessions;

/// <summary>
/// Represents an IRC client session
/// </summary>
public class IrcSession
{
    #region Identity Properties

    /// <summary>
    /// Unique identifier for this session
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Client's IP address
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// Client's resolved hostname
    /// </summary>
    public string HostName { get; set; }


    /// <summary>
    ///  Client's virtual hostname (if set)
    /// </summary>
    public string? VirtualHostName { get; set; }

    /// <summary>
    /// Client's connection port
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Client's chosen nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// Client's username (ident)
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Client's real name
    /// </summary>
    public string RealName { get; set; }

    /// <summary>
    /// Gets the full user mask (nick!user@host)
    /// </summary>
    public string UserMask => $"{Nickname}!{Username}@{VirtualHostName ?? HostName}";

    /// <summary>
    ///  Whether the session is valid (has a nickname and username)
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(Nickname) && !string.IsNullOrEmpty(Username);

    #endregion

    #region State Properties

    /// <summary>
    ///  Whether the client has sent a USER command
    /// </summary>
    public bool IsUserSent { get; set; }


    /// <summary>
    ///  Whether the client has sent a NICK command
    /// </summary>
    public bool IsNickSent { get; set; }

    /// <summary>
    ///  Whether the client has sent a PASS command
    /// </summary>
    public bool IsPasswordSent { get; set; }


    /// <summary>
    /// Whether the client has completed registration
    /// </summary>
    public bool IsRegistered => IsUserSent && IsNickSent && IsPasswordSent;

    /// <summary>
    /// Whether the client is marked as away
    /// </summary>
    public bool IsAway { get; private set; }

    /// <summary>
    /// The client's away message
    /// </summary>
    public string AwayMessage { get; private set; }

    /// <summary>
    /// Timestamp of last PING sent to client
    /// </summary>
    public DateTime LastPingSent { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of last PONG received from client
    /// </summary>
    public DateTime LastPongReceived { get; set; }

    /// <summary>
    /// Timestamp of last activity (any message received)
    /// </summary>
    public DateTime LastActivity { get; set; }

    /// <summary>
    /// Whether a PING is pending response
    /// </summary>
    public bool IsPingPending { get; set; }

    #endregion

    #region Channels and Modes

    /// <summary>
    /// Channels the client has joined
    /// </summary>
    private readonly ConcurrentDictionary<string, ChannelMembership> _channels = new();

    /// <summary>
    /// User modes applied to this client
    /// </summary>
    private readonly HashSet<char> _userModes = new();

    /// <summary>
    /// Gets a read-only list of channel names the client has joined
    /// </summary>
    public IReadOnlyCollection<string> JoinedChannels => _channels.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Gets the user modes as a string
    /// </summary>
    public string ModesString => new string(_userModes.ToArray());

    #endregion

    #region Common Mode Properties

    /// <summary>
    /// Whether the client is invisible (mode +i)
    /// </summary>
    public bool IsInvisible => HasMode('i');

    /// <summary>
    /// Whether the client is an IRC operator (mode +o)
    /// </summary>
    public bool IsOperator => HasMode('o');

    /// <summary>
    /// Whether the client receives wallops messages (mode +w)
    /// </summary>
    public bool ReceivesWallops => HasMode('w');

    /// <summary>
    /// Whether the client is registered with services (mode +r)
    /// </summary>
    public bool IsRegisteredUser => HasMode('r');

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new IRC session
    /// </summary>
    public IrcSession(string id, string ipAddress, int port, string hostname = null)
    {
        Id = id;
        IpAddress = ipAddress;
        Port = port;
        HostName = hostname ?? ipAddress;
        LastActivity = DateTime.Now;
        LastPingSent = DateTime.Now;
        LastPongReceived = DateTime.Now;
    }

    #endregion

    #region Channel Methods

    /// <summary>
    /// Adds a channel to the client's joined channels list
    /// </summary>
    public void JoinChannel(string channelName)
    {
        _channels.TryAdd(channelName.ToLowerInvariant(), new ChannelMembership());
    }

    /// <summary>
    /// Removes a channel from the client's joined channels list
    /// </summary>
    public bool LeaveChannel(string channelName)
    {
        return _channels.TryRemove(channelName.ToLowerInvariant(), out _);
    }

    /// <summary>
    /// Checks if the client is in a specific channel
    /// </summary>
    public bool IsInChannel(string channelName)
    {
        return _channels.ContainsKey(channelName.ToLowerInvariant());
    }

    /// <summary>
    /// Gets the client's membership status in a channel
    /// </summary>
    public ChannelMembership GetChannelMembership(string channelName)
    {
        return _channels.GetValueOrDefault(channelName.ToLowerInvariant());
    }

    #endregion

    #region Mode Methods

    /// <summary>
    /// Checks if the client has a specific user mode
    /// </summary>
    public bool HasMode(char mode)
    {
        return _userModes.Contains(mode);
    }

    /// <summary>
    /// Adds a user mode to the client
    /// </summary>
    public bool AddMode(char mode)
    {
        return _userModes.Add(mode);
    }

    /// <summary>
    /// Removes a user mode from the client
    /// </summary>
    public bool RemoveMode(char mode)
    {
        return _userModes.Remove(mode);
    }

    /// <summary>
    /// Applies a mode change string to the client
    /// </summary>
    public void ApplyModeChanges(string modeString)
    {
        if (string.IsNullOrEmpty(modeString))
        {
            return;
        }

        char action = '+';

        foreach (char c in modeString)
        {
            if (c == '+' || c == '-')
            {
                action = c;
            }
            else
            {
                if (action == '+')
                    AddMode(c);
                else if (action == '-')
                    RemoveMode(c);
            }
        }
    }

    #endregion

    #region Away Status Methods

    /// <summary>
    /// Marks the client as away with a message
    /// </summary>
    public void SetAway(string message)
    {
        IsAway = true;
        AwayMessage = message;
    }

    /// <summary>
    /// Marks the client as back (not away)
    /// </summary>
    public void SetBack()
    {
        IsAway = false;
        AwayMessage = null;
    }

    #endregion

    #region Activity Methods

    /// <summary>
    /// Updates the last activity timestamp
    /// </summary>
    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }

    #endregion
}
