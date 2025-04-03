using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
/// Represents RPL_ENDOFWHO (315) numeric reply
/// Sent at the end of WHO command responses to mark the end of the list
/// Format: ":server 315 nickname mask :End of WHO list"
/// </summary>
public class RplEndOfWho : BaseIrcCommand
{
    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client that sent the WHO request
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The mask that was used in the WHO query
    /// </summary>
    public string Mask { get; set; }

    /// <summary>
    /// The end message (default: "End of WHO list")
    /// </summary>
    public string Message { get; set; } = "End of WHO list";

    public RplEndOfWho() : base("315")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.example.com 315 nickname somemask :End of WHO list
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "315"
        Nickname = parts[2];
        Mask = parts[3];

        // Extract message (removes the leading ":")
        Message = parts[4].StartsWith(':') ? parts[4].Substring(1) : parts[4];
    }

    public override string Write()
    {
        return $":{ServerName} 315 {Nickname} {Mask} :{Message}";
    }

    /// <summary>
    /// Creates an RPL_ENDOFWHO reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">Nickname of the client that sent the WHO request</param>
    /// <param name="mask">Mask that was used in the WHO query</param>
    /// <param name="message">Optional custom end message</param>
    /// <returns>A formatted RPL_ENDOFWHO response</returns>
    public static RplEndOfWho Create(
        string serverName,
        string nickname,
        string mask,
        string message = null)
    {
        return new RplEndOfWho
        {
            ServerName = serverName,
            Nickname = nickname,
            Mask = mask,
            Message = message ?? "End of WHO list"
        };
    }
}
