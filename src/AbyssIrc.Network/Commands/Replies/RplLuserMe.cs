using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_LUSERME (255) numeric reply showing local connection info
/// </summary>
public class RplLuserMe : BaseIrcCommand
{
    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// Number of clients connected to this server
    /// </summary>
    public int ClientCount { get; set; }

    /// <summary>
    /// Number of servers connected to this server
    /// </summary>
    public int ServerCount { get; set; }

    public RplLuserMe() : base("255")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.com 255 nickname :I have 5 clients and 3 servers
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "255"
        Nickname = parts[2];

        // Parse client and server counts from message
        string message = parts[3].TrimStart(':');
        try
        {
            var matches = System.Text.RegularExpressions.Regex.Match(
                message,
                @"I have (\d+) clients and (\d+) servers");

            if (matches.Success && matches.Groups.Count >= 3)
            {
                ClientCount = int.Parse(matches.Groups[1].Value);
                ServerCount = int.Parse(matches.Groups[2].Value);
            }
        }
        catch
        {
            // Parsing failed
        }
    }

    public override string Write()
    {
        return $":{ServerName} 255 {Nickname} :I have {ClientCount} clients and {ServerCount} servers";
    }

    /// <summary>
    /// Creates an RPL_LUSERME reply
    /// </summary>
    public static RplLuserMe Create(string serverName, string nickname, int clientCount, int serverCount)
    {
        return new RplLuserMe
        {
            ServerName = serverName,
            Nickname = nickname,
            ClientCount = clientCount,
            ServerCount = serverCount
        };
    }
}
