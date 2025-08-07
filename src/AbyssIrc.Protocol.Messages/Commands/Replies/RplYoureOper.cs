using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_YOUREOPER (381) numeric reply
///     Sent when a client successfully authenticates as an IRC operator
///     Format: ":server 381 nickname :You are now an IRC operator"
/// </summary>
public class RplYoureOper : BaseIrcCommand
{
    public RplYoureOper() : base("381")
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
    ///     The success message (default: "You are now an IRC operator")
    /// </summary>
    public string Message { get; set; } = "You are now an IRC operator";

    public override void Parse(string line)
    {
        // Example: :irc.server.net 381 nickname :You are now an IRC operator
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "381"
        Nickname = parts[2];

        // Extract message (removes the leading ":")
        Message = parts[3].StartsWith(':') ? parts[3].Substring(1) : parts[3];
    }

    public override string Write()
    {
        return $":{ServerName} 381 {Nickname} :{Message}";
    }

    /// <summary>
    ///     Creates an RPL_YOUREOPER reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="message">Optional custom message (defaults to standard message)</param>
    /// <returns>A formatted RPL_YOUREOPER response</returns>
    public static RplYoureOper Create(
        string serverName,
        string nickname,
        string message = null
    )
    {
        return new RplYoureOper
        {
            ServerName = serverName,
            Nickname = nickname,
            Message = message ?? "You are now an IRC operator"
        };
    }
}
