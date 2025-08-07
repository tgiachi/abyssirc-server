using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents ERR_BADCHANMASK (476) numeric reply
///     Sent when a client provides an invalid channel name (bad mask)
///     Format: ":server 476 nickname #channel :Bad Channel Mask"
/// </summary>
public class ErrBadChanMask : BaseIrcCommand
{
    public ErrBadChanMask() : base("476")
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
    ///     The invalid channel name
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The error message
    /// </summary>
    public string ErrorMessage { get; set; } = "Bad Channel Mask";

    public override void Parse(string line)
    {
        // Example: :irc.server.net 476 nickname #bad?channel :Bad Channel Mask
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "476"
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
        return $":{ServerName} 476 {Nickname} {ChannelName} :{ErrorMessage}";
    }

    /// <summary>
    ///     Creates an ERR_BADCHANMASK reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="channelName">The invalid channel name</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>A formatted ERR_BADCHANMASK response</returns>
    public static ErrBadChanMask Create(
        string serverName,
        string nickname,
        string channelName,
        string errorMessage = "Bad Channel Mask"
    )
    {
        return new ErrBadChanMask
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            ErrorMessage = errorMessage
        };
    }
}
