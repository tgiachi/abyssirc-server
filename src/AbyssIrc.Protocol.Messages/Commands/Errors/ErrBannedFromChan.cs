using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
/// Represents ERR_BANNEDFROMCHAN (474) numeric reply
/// Sent when a client tries to join a channel they are banned from
/// Format: ":server 474 nickname #channel :Cannot join channel (+b) - you are banned"
/// </summary>
public class ErrBannedFromChan : BaseIrcCommand
{
    public ErrBannedFromChan() : base("474")
    {
    }

    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The channel name the client is banned from
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// The error message
    /// </summary>
    public string ErrorMessage { get; set; } = "Cannot join channel (+b) - you are banned";

    public override void Parse(string line)
    {
        // Example: :irc.server.net 474 nickname #channel :Cannot join channel (+b) - you are banned
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "474"
        Nickname = parts[2];
        ChannelName = parts[3];

        // Extract error message (removes the leading ":")
        if (parts[4].StartsWith(':'))
        {
            ErrorMessage = parts[4].Substring(1);
        }
        else
        {
            ErrorMessage = parts[4];
        }
    }

    public override string Write()
    {
        return $":{ServerName} 474 {Nickname} {ChannelName} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_BANNEDFROMCHAN reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="channelName">The channel name the client is banned from</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>A formatted ERR_BANNEDFROMCHAN response</returns>
    public static ErrBannedFromChan Create(
        string serverName,
        string nickname,
        string channelName,
        string errorMessage = "Cannot join channel (+b) - you are banned")
    {
        return new ErrBannedFromChan
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            ErrorMessage = errorMessage
        };
    }
}
