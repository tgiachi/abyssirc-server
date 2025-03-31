using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_ENDOFINFO (374) numeric reply
/// Indicates the end of server information lines
/// </summary>
public class RplEndOfInfo : BaseIrcCommand
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
    /// Optional end of info message
    /// </summary>
    public string Message { get; set; } = "End of /INFO list";

    public RplEndOfInfo() : base("374")
    {
    }

    /// <summary>
    /// Parses the RPL_ENDOFINFO numeric reply
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 374 nickname :End of /INFO list

        // Reset existing data
        ServerName = null;
        Nickname = null;
        Message = "End of /INFO list";

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
        if (parts[0] != "374")
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
    /// <returns>Formatted RPL_ENDOFINFO message</returns>
    public override string Write()
    {
        return string.IsNullOrEmpty(ServerName)
            ? $"374 {Nickname} :{Message}"
            : $":{ServerName} 374 {Nickname} :{Message}";
    }

    /// <summary>
    /// Creates a RPL_ENDOFINFO reply
    /// </summary>
    /// <param name="serverName">Server sending the reply</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="message">Optional end of info message</param>
    public static RplEndOfInfo Create(
        string serverName,
        string nickname,
        string message = null
    )
    {
        return new RplEndOfInfo
        {
            ServerName = serverName,
            Nickname = nickname,
            Message = message ?? "End of /INFO list"
        };
    }
}
