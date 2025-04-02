using System.Collections.Concurrent;
using AbyssIrc.Network.Types;
using AbyssIrc.Server.Data.Channels;

namespace AbyssIrc.Network.Data.Channels;

public class ChannelData
{
    #region Properties

    /// <summary>
    /// The channel name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The channel topic
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// When the channel was created
    /// </summary>
    public DateTime CreationTime { get; }

    /// <summary>
    /// When the topic was last changed
    /// </summary>
    public DateTime TopicSetTime { get; set; }

    /// <summary>
    /// Who set the topic (nick!user@host)
    /// </summary>
    public string TopicSetBy { get; set; } = string.Empty;

    /// <summary>
    /// Channel mode flags
    /// </summary>
    private readonly HashSet<char> _modes = new();

    /// <summary>
    /// Channel mode parameters (for modes that take parameters)
    /// </summary>
    private readonly ConcurrentDictionary<char, string> _modeParameters = new();

    /// <summary>
    /// Channel members with their membership status
    /// </summary>
    private readonly ConcurrentDictionary<string, ChannelMembership> _members = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Ban masks
    /// </summary>
    private readonly ConcurrentDictionary<string, BanEntry> _banList = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Invite exceptions (mode +I)
    /// </summary>
    private readonly ConcurrentDictionary<string, BanEntry> _invexList = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Ban exceptions (mode +e)
    /// </summary>
    private readonly ConcurrentDictionary<string, BanEntry> _exceptList = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The channel key/password if set (mode +k)
    /// </summary>
    public string Key => GetModeParameter('k');

    /// <summary>
    /// The channel user limit if set (mode +l)
    /// </summary>
    public int? UserLimit
    {
        get
        {
            var limitStr = GetModeParameter('l');
            if (string.IsNullOrEmpty(limitStr) || !int.TryParse(limitStr, out var limit))
            {
                return null;
            }

            return limit;
        }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new channel with the specified name
    /// </summary>
    /// <param name="name">The channel name (including prefix like # or &)</param>
    public ChannelData(string name)
    {
        Name = name;
        CreationTime = DateTime.UtcNow;
        TopicSetTime = DateTime.UtcNow;
    }

    #endregion

    #region Mode Methods

    /// <summary>
    /// Checks if the channel has a specific mode
    /// </summary>
    /// <param name="mode">The mode character</param>
    /// <returns>True if the mode is set</returns>
    public bool HasMode(char mode) => _modes.Contains(mode);

    /// <summary>
    /// Sets a mode on the channel
    /// </summary>
    /// <param name="mode">The mode to set</param>
    /// <param name="parameter">Optional parameter for the mode</param>
    /// <returns>True if the mode was newly set</returns>
    public bool SetMode(char mode, string parameter = null)
    {
        var added = _modes.Add(mode);

        if (!string.IsNullOrEmpty(parameter))
        {
            _modeParameters[mode] = parameter;
        }

        return added;
    }

    /// <summary>
    ///  Sets the channel to topic protection mode (mode +t)
    /// </summary>
    /// <returns></returns>
    public bool SetTopicProtection()
    {
        if (HasMode('t'))
        {
            return false;
        }

        SetMode('t');
        return true;
    }


    /// <summary>
    ///  Anti-spam control (mode +C)
    /// </summary>
    /// <returns></returns>
    public bool SetAntiSpamControl()
    {
        if (HasMode('C'))
        {
            return false;
        }

        SetMode('C');
        return true;
    }

    /// <summary>
    ///  Anti-spam control (mode +C)
    /// </summary>
    /// <returns></returns>
    public bool RemoveAntiSpamControl()
    {
        if (!HasMode('C'))
        {
            return false;
        }

        RemoveMode('C');
        return true;
    }

    /// <summary>
    ///  Sets the channel to only allow messages from users with voice (mode +n)
    /// </summary>
    /// <returns></returns>
    public bool SetOnlyForPresents()
    {
        if (HasMode('n'))
        {
            return false;
        }

        SetMode('n');
        return true;
    }

    /// <summary>
    ///  Removes the topic protection mode (mode -t)
    /// </summary>
    /// <returns></returns>
    public bool RemoveTopicProtection()
    {
        if (!HasMode('t'))
        {
            return false;
        }

        RemoveMode('t');
        return true;
    }

    /// <summary>
    /// Removes a mode from the channel
    /// </summary>
    /// <param name="mode">The mode to remove</param>
    /// <returns>True if the mode was removed</returns>
    public bool RemoveMode(char mode)
    {
        _modeParameters.TryRemove(mode, out _);
        return _modes.Remove(mode);
    }

    /// <summary>
    /// Gets the parameter for a specific mode
    /// </summary>
    /// <param name="mode">The mode character</param>
    /// <returns>The parameter or empty string if not set</returns>
    public string GetModeParameter(char mode)
    {
        return _modeParameters.TryGetValue(mode, out var value) ? value : string.Empty;
    }


    /// <summary>
    ///  Gets the list of mode changes to be applied
    /// </summary>
    /// <returns></returns>
    public ModeChangeType[] GetModeChanges()
    {
        var changes = new List<ModeChangeType>();

        foreach (var mode in _modes)
        {
            changes.Add(
                new ModeChangeType
                {
                    IsAdding = true,
                    Mode = mode,
                    Parameter = GetModeParameter(mode)
                }
            );
        }

        return changes.ToArray();
    }

    /// <summary>
    /// Gets the channel modes as a string (e.g., "+ntk" or "+sml 10")
    /// </summary>
    /// <returns>A string representation of the modes</returns>
    public string GetModeString()
    {
        if (_modes.Count == 0)
        {
            return string.Empty;
        }

        var modeStr = "+" + string.Concat(_modes.OrderBy(c => c));
        var parameters = new List<string>();

        // Add parameters for modes that require them
        foreach (var mode in _modes)
        {
            if (_modeParameters.TryGetValue(mode, out var param) && !string.IsNullOrEmpty(param))
            {
                parameters.Add(param);
            }
        }

        if (parameters.Count > 0)
        {
            modeStr += " " + string.Join(" ", parameters);
        }

        return modeStr;
    }

    /// <summary>
    /// Applies a mode change string to the channel
    /// </summary>
    /// <param name="modeString">Mode string like "+nt-s"</param>
    /// <param name="parameters">Optional parameters for the modes</param>
    /// <returns>List of mode changes that were applied</returns>
    public List<ModeChange> ApplyModeChanges(string modeString, List<string> parameters = null)
    {
        if (string.IsNullOrEmpty(modeString))
        {
            return new List<ModeChange>();
        }

        var changes = new List<ModeChange>();
        var action = '+';
        var paramIndex = 0;

        foreach (var c in modeString)
        {
            if (c == '+' || c == '-')
            {
                action = c;
                continue;
            }

            string parameter = null;

            // Check if this mode takes a parameter
            bool requiresParam = false;

            // Mode +k (key) always requires a parameter when adding
            if (c == 'k' && action == '+')
            {
                requiresParam = true;
            }
            // Mode +l (limit) always requires a parameter when adding
            else if (c == 'l' && action == '+')
            {
                requiresParam = true;
            }
            // Ban lists/invex/except
            else if (c == 'b' || c == 'I' || c == 'e')
            {
                requiresParam = true;
            }

            if (requiresParam && parameters != null && paramIndex < parameters.Count)
            {
                parameter = parameters[paramIndex++];
            }

            if (action == '+')
            {
                if (SetMode(c, parameter))
                {
                    changes.Add(new ModeChange(true, c, parameter));
                }
            }
            else if (action == '-')
            {
                if (RemoveMode(c))
                {
                    changes.Add(new ModeChange(false, c, parameter));
                }
            }
        }

        return changes;
    }

    #endregion

    #region Member Management

    /// <summary>
    /// Adds a user to the channel
    /// </summary>
    /// <param name="nickname">The user's nickname</param>
    /// <returns>The created membership or null if already in channel</returns>
    public ChannelMembership AddMember(string nickname)
    {
        if (IsMember(nickname))
        {
            return null;
        }

        var membership = new ChannelMembership();
        return _members.TryAdd(nickname, membership) ? membership : null;
    }

    /// <summary>
    /// Removes a user from the channel
    /// </summary>
    /// <param name="nickname">The user's nickname</param>
    /// <returns>True if the user was removed</returns>
    public bool RemoveMember(string nickname)
    {
        return _members.TryRemove(nickname, out _);
    }

    /// <summary>
    /// Checks if a user is a member of the channel
    /// </summary>
    /// <param name="nickname">The user's nickname</param>
    /// <returns>True if the user is a member</returns>
    public bool IsMember(string nickname)
    {
        return _members.ContainsKey(nickname);
    }

    public bool IsOperator(string nickname)
    {
        return _members.TryGetValue(nickname, out var membership) && membership.IsOperator;
    }

    /// <summary>
    /// Gets the membership information for a user
    /// </summary>
    /// <param name="nickname">The user's nickname</param>
    /// <returns>The membership or null if not a member</returns>
    public ChannelMembership GetMembership(string nickname)
    {
        return _members.TryGetValue(nickname, out var membership) ? membership : null;
    }

    /// <summary>
    /// Gets a read-only list of all members in the channel
    /// </summary>
    /// <returns>List of members</returns>
    public IReadOnlyCollection<string> GetMemberList()
    {
        return _members.Keys.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the count of members in the channel
    /// </summary>
    public int MemberCount => _members.Count;

    /// <summary>
    /// Gets the members with operator status
    /// </summary>
    /// <returns>List of operators</returns>
    public IReadOnlyCollection<string> GetOperators()
    {
        return _members
            .Where(m => m.Value.IsOperator)
            .Select(m => m.Key)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the members with voice status
    /// </summary>
    /// <returns>List of voiced users</returns>
    public IReadOnlyCollection<string> GetVoiced()
    {
        return _members
            .Where(m => m.Value.HasVoice)
            .Select(m => m.Key)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Sets or removes operator status for a user
    /// </summary>
    /// <param name="nickname">The nickname to modify</param>
    /// <param name="isOp">Whether to give or remove op</param>
    /// <returns>True if changed, false if user not in channel</returns>
    public bool SetOperator(string nickname, bool isOp)
    {
        if (_members.TryGetValue(nickname, out var membership))
        {
            membership.IsOperator = isOp;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets or removes voice status for a user
    /// </summary>
    /// <param name="nickname">The nickname to modify</param>
    /// <param name="hasVoice">Whether to give or remove voice</param>
    /// <returns>True if changed, false if user not in channel</returns>
    public bool SetVoice(string nickname, bool hasVoice)
    {
        if (_members.TryGetValue(nickname, out var membership))
        {
            membership.HasVoice = hasVoice;
            return true;
        }

        return false;
    }

    #endregion

    #region Invites

    /// <summary>
    ///  /// The list of users who are allowed to invite others to the channel
    /// </summary>
    public List<string> InviteList { get; } = new();


    public void AddInvite(string nickname)
    {
        if (!InviteList.Contains(nickname))
        {
            InviteList.Add(nickname);
        }
    }

    public void RemoveInvite(string nickname)
    {
        if (InviteList.Contains(nickname))
        {
            InviteList.Remove(nickname);
        }
    }

    public bool IsInvited(string nickname)
    {
        return InviteList.Contains(nickname);
    }


    public bool NickNameCanJoin(string nickname)
    {
        if (IsInviteOnly && !IsInvited(nickname))
        {
            return false;
        }



        return true;
    }

    #endregion

    #region Ban Management

    /// <summary>
    /// Adds a ban mask to the channel
    /// </summary>
    /// <param name="mask">The ban mask (e.g., *!*@example.com)</param>
    /// <param name="setBy">Who set the ban</param>
    /// <returns>True if the ban was newly added</returns>
    public bool AddBan(string mask, string setBy)
    {
        var ban = new BanEntry(mask, setBy, DateTime.UtcNow);
        return _banList.TryAdd(mask, ban);
    }

    /// <summary>
    /// Removes a ban mask from the channel
    /// </summary>
    /// <param name="mask">The ban mask to remove</param>
    /// <returns>True if the ban was removed</returns>
    public bool RemoveBan(string mask)
    {
        return _banList.TryRemove(mask, out _);
    }

    /// <summary>
    /// Gets the list of bans
    /// </summary>
    /// <returns>Collection of ban entries</returns>
    public IReadOnlyCollection<BanEntry> GetBans()
    {
        return _banList.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Adds an invite exception to the channel
    /// </summary>
    /// <param name="mask">The exception mask</param>
    /// <param name="setBy">Who set the exception</param>
    /// <returns>True if added</returns>
    public bool AddInviteException(string mask, string setBy)
    {
        var exception = new BanEntry(mask, setBy, DateTime.UtcNow);
        return _invexList.TryAdd(mask, exception);
    }

    /// <summary>
    /// Removes an invite exception
    /// </summary>
    /// <param name="mask">The mask to remove</param>
    /// <returns>True if removed</returns>
    public bool RemoveInviteException(string mask)
    {
        return _invexList.TryRemove(mask, out _);
    }

    /// <summary>
    /// Gets all invite exceptions
    /// </summary>
    public IReadOnlyCollection<BanEntry> GetInviteExceptions()
    {
        return _invexList.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Adds a ban exception to the channel
    /// </summary>
    /// <param name="mask">The exception mask</param>
    /// <param name="setBy">Who set the exception</param>
    /// <returns>True if added</returns>
    public bool AddBanException(string mask, string setBy)
    {
        var exception = new BanEntry(mask, setBy, DateTime.UtcNow);
        return _exceptList.TryAdd(mask, exception);
    }

    /// <summary>
    /// Removes a ban exception
    /// </summary>
    /// <param name="mask">The mask to remove</param>
    /// <returns>True if removed</returns>
    public bool RemoveBanException(string mask)
    {
        return _exceptList.TryRemove(mask, out _);
    }

    /// <summary>
    /// Gets all ban exceptions
    /// </summary>
    public IReadOnlyCollection<BanEntry> GetBanExceptions()
    {
        return _exceptList.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the nickname with appropriate prefix based on member status
    /// @ for channel operators, + for voiced users, and none for regular members
    /// </summary>
    /// <param name="nickname">The user's nickname</param>
    /// <returns>The nickname with appropriate prefix or null if user is not in channel</returns>
    public string GetPrefixedNickname(string nickname)
    {
        if (!IsMember(nickname))
        {
            return null;
        }

        var membership = GetMembership(nickname);

        // Apply IRC standard prefixes
        if (membership.IsOperator)
        {
            return "@" + nickname;
        }

        if (membership.HasVoice)
        {
            return "+" + nickname;
        }

        return nickname;
    }

    /// <summary>
    /// Gets all channel members with their appropriate prefix
    /// </summary>
    /// <returns>A list of channel members with prefixes</returns>
    public List<string> GetPrefixedMemberList()
    {
        return _members.Keys
            .Select(GetPrefixedNickname)
            .Where(nick => nick != null)
            .ToList();
    }


    /// <summary>
    /// Extracts prefixed nicknames from a list of usermasks.
    /// </summary>
    /// <param name="usermasks">List of usermasks in the format nickname!user@host</param>
    /// <returns>List of nicknames with appropriate prefix based on channel status</returns>
    public List<string> GetUserMaskList(List<string> usermasks)
    {
        // Extract user from usermask and get prefixed nickname
        // and rebuild like @nickname!user@host

        var prefixedNicknames = new List<string>();

        foreach (var usermask in usermasks)
        {
            // Split the usermask to get the nickname part
            int exclamationIndex = usermask.IndexOf('!');
            if (exclamationIndex <= 0)
            {
                // Invalid usermask, skip
                continue;
            }

            // Extract the nickname
            string nickname = usermask[..exclamationIndex];

            // Determine the appropriate prefix based on user's role
            string prefix = "";


            var membership = GetMembership(nickname);

            // Assign prefix based on channel status
            if (membership != null)
            {
                if (membership.IsOperator)
                {
                    prefix = "@"; // Channel operator
                }
                else if (membership.HasVoice)
                {
                    prefix = "+"; // Voiced user
                }
            }


            // Add the prefixed nickname to the list
            prefixedNicknames.Add($"{prefix}{usermask}");
        }

        return prefixedNicknames;
    }

    #endregion

    #region Channel Properties (based on modes)

    /// <summary>
    /// Whether the channel is secret (mode +s)
    /// </summary>
    public bool IsSecret => HasMode('s');

    /// <summary>
    /// Whether the channel is private (mode +p)
    /// </summary>
    public bool IsPrivate => HasMode('p');

    /// <summary>
    /// Whether the channel is invite-only (mode +i)
    /// </summary>
    public bool IsInviteOnly => HasMode('i');

    /// <summary>
    /// Whether the channel has a key/password set (mode +k)
    /// </summary>
    public bool HasKey => HasMode('k');

    /// <summary>
    /// Whether the channel has a user limit set (mode +l)
    /// </summary>
    public bool HasUserLimit => HasMode('l');

    /// <summary>
    /// Whether the channel is moderated (mode +m)
    /// </summary>
    public bool IsModerated => HasMode('m');


    /// <summary>
    /// Whether the channel has no external messages (mode +n)
    /// </summary>
    public bool NoExternalMessages => HasMode('n');

    /// <summary>
    /// Whether the channel topic can only be changed by operators (mode +t)
    /// </summary>
    public bool TopicProtection => HasMode('t');


    /// <summary>
    /// Get if the channel has a topic set
    /// </summary>
    public bool HaveTopic => !string.IsNullOrEmpty(Topic);

    #endregion
}
