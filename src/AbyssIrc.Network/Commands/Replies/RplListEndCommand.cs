using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_LISTEND (323) numeric reply
/// Indicates the end of a channel list
/// </summary>
public class RplListEndCommand : BaseIrcCommand
{
    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// Optional end of list message
    /// </summary>
    public string Message { get; set; } = "End of /LIST";

    public RplListEndCommand() : base("323")
    {
    }

    /// <summary>
    /// Parses the RPL_LISTEND numeric reply
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 323 nickname :End of /LIST

        // Reset existing data
        ServerName = null;
        Nickname = null;
        Message = "End of /LIST";

        // Check for source prefix
        if (line.StartsWith(':'))
        {
            int spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                ServerName = line.Substring(1, spaceIndex - 1);
                line = line.Substring(spaceIndex + 1).TrimStart();
            }
        }

        // Split remaining parts
        string[] parts = line.Split(' ');

        // Ensure we have enough parts
        if (parts.Length < 2)
            return;

        // Verify the numeric code
        if (parts[0] != "323")
            return;

        // Extract nickname
        Nickname = parts[1];

        // Extract message if present
        int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + 2);
        if (colonIndex != -1)
        {
            Message = line.Substring(colonIndex + 1);
        }
    }

    /// <summary>
    /// Converts the reply to its string representation
    /// </summary>
    /// <returns>Formatted RPL_LISTEND message</returns>
    public override string Write()
    {
        return string.IsNullOrEmpty(ServerName)
            ? $"323 {Nickname} :{Message}"
            : $":{ServerName} 323 {Nickname} :{Message}";
    }

    /// <summary>
    /// Creates a RPL_LISTEND reply
    /// </summary>
    /// <param name="serverName">Server sending the reply</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="message">Optional end of list message</param>
    public static RplListEndCommand Create(
        string serverName,
        string nickname,
        string message = null
    )
    {
        return new RplListEndCommand
        {
            ServerName = serverName,
            Nickname = nickname,
            Message = message ?? "End of /LIST"
        };
    }
}
