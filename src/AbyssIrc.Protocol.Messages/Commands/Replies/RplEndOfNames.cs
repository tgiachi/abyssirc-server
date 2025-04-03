using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
/// Represents RPL_ENDOFNAMES (366) numeric reply that marks the end of a NAMES list
/// Format: ":server 366 nickname #channel :End of /NAMES list"
/// </summary>
public class RplEndOfNames : BaseIrcCommand
{
    public RplEndOfNames() : base("366")
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
    /// The channel name
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// Optional custom message (default: "End of /NAMES list")
    /// </summary>
    public string Message { get; set; } = "End of /NAMES list";

    public override void Parse(string line)
    {
        // Example: :irc.server.net 366 MyNick #channel :End of /NAMES list
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "366"
        Nickname = parts[2];
        ChannelName = parts[3];

        // Extract message (removes the leading ":")
        if (parts[4].StartsWith(':'))
        {
            Message = parts[4].Substring(1);
        }
        else
        {
            Message = parts[4];
        }
    }

    public override string Write()
    {
        return $":{ServerName} 366 {Nickname} {ChannelName} :{Message}";
    }

    /// <summary>
    /// Creates an RPL_ENDOFNAMES response
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="channelName">The channel name</param>
    /// <param name="message">Optional custom message</param>
    /// <returns>A formatted RPL_ENDOFNAMES response</returns>
    public static RplEndOfNames Create(
        string serverName,
        string nickname,
        string channelName,
        string message = "End of /NAMES list"
    )
    {
        return new RplEndOfNames
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            Message = message
        };
    }
}
