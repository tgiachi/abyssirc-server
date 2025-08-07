using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents ERR_BADCHANNELKEY (475) numeric reply
///     Sent when a client tries to join a channel that requires a key (mode +k) with an incorrect key
///     Format: ":server 475 nickname #channel :Cannot join channel (+k) - bad key"
/// </summary>
public class ErrBadChannelKey : BaseIrcCommand
{
    public ErrBadChannelKey() : base("475")
    {
    }

    /// <summary>
    ///     The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The channel name that requires a key
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The error message
    /// </summary>
    public string ErrorMessage { get; set; } = "Cannot join channel (+k) - bad key";

    public override void Parse(string line)
    {
        // Example: :irc.server.net 475 nickname #channel :Cannot join channel (+k) - bad key
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "475"
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
        return $":{ServerName} 475 {Nickname} {ChannelName} :{ErrorMessage}";
    }

    /// <summary>
    ///     Creates an ERR_BADCHANNELKEY reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="channelName">The channel name that requires a key</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>A formatted ERR_BADCHANNELKEY response</returns>
    public static ErrBadChannelKey Create(
        string serverName,
        string nickname,
        string channelName,
        string errorMessage = "Cannot join channel (+k) - bad key"
    )
    {
        return new ErrBadChannelKey
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            ErrorMessage = errorMessage
        };
    }
}
