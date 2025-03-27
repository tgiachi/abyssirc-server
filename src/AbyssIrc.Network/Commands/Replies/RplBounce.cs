using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_BOUNCE (005) numeric reply used to redirect clients to another server
/// </summary>
public class RplBounce : BaseIrcCommand
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
    /// The hostname of the server to connect to
    /// </summary>
    public string TargetServer { get; set; }

    /// <summary>
    /// The port of the server to connect to
    /// </summary>
    public int TargetPort { get; set; }

    /// <summary>
    /// Additional message to send to the client
    /// </summary>
    public string Message { get; set; } = "Try server %s, port %d";

    public RplBounce() : base("005")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.com 005 nickname :Try server other.server.com, port 6667
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "005"
        Nickname = parts[2];

        // Extract server and port from message
        string fullMessage = parts[3].TrimStart(':');
        Message = fullMessage;

        // Try to extract server and port information
        try
        {
            // This is a simplistic parser for demonstration
            // For a real implementation, you'd want a more robust approach
            int serverIndex = fullMessage.IndexOf("server ") + 7;
            int commaIndex = fullMessage.IndexOf(',', serverIndex);

            if (serverIndex > 6 && commaIndex > serverIndex)
            {
                TargetServer = fullMessage.Substring(serverIndex, commaIndex - serverIndex).Trim();

                int portIndex = fullMessage.IndexOf("port ") + 5;
                if (portIndex > 4)
                {
                    string portStr = fullMessage.Substring(portIndex).Trim();
                    if (int.TryParse(portStr, out int port))
                    {
                        TargetPort = port;
                    }
                }
            }
        }
        catch
        {
            // Parsing failed, but we still have the message
        }
    }

    public override string Write()
    {
        // Format the message with server and port
        string formattedMessage = Message;

        // If the message contains formatting placeholders, replace them
        if (Message.Contains("%s") && Message.Contains("%d"))
        {
            formattedMessage = string.Format(
                Message.Replace("%s", "{0}").Replace("%d", "{1}"),
                TargetServer,
                TargetPort
            );
        }

        return $":{ServerName} 005 {Nickname} :{formattedMessage}";
    }

    /// <summary>
    /// Creates an RPL_BOUNCE reply
    /// </summary>
    public static RplBounce Create(
        string serverName, string nickname, string targetServer, int targetPort, string message = null
    )
    {
        return new RplBounce
        {
            ServerName = serverName,
            Nickname = nickname,
            TargetServer = targetServer,
            TargetPort = targetPort,
            Message = message ?? "Try server %s, port %d"
        };
    }
}
