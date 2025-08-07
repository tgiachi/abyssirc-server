using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents an IRC PONG command used as a response to PING
/// </summary>
public class PongCommand : BaseIrcCommand
{
    public PongCommand() : base("PONG")
    {
    }

    public PongCommand(string source, string token) : base("PONG")
    {
        Source = source;
        Token = token;
    }

    /// <summary>
    ///     The token/parameter included in the PONG
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    ///     The source of the PONG (typically server name if sent by server)
    /// </summary>
    public string Source { get; set; }

    public override void Parse(string line)
    {
        // Examples:
        // Client to server: PONG :token
        // Server to client: :server.com PONG :token

        var parts = line.Split(' ');

        if (parts[0].StartsWith(":"))
        {
            // Server to client format
            Source = parts[0].TrimStart(':');

            if (parts.Length > 2)
            {
                Token = parts[2].TrimStart(':');
            }
        }
        else
        {
            // Client to server format
            if (parts.Length > 1)
            {
                Token = parts[1].TrimStart(':');
            }
        }
    }

    public override string Write()
    {
        if (!string.IsNullOrEmpty(Source))
        {
            return $":{Source} PONG :{Token}";
        }

        return $"PONG :{Token}";
    }

    /// <summary>
    ///     Creates a PONG command from server to client
    /// </summary>
    public static PongCommand CreateFromServer(string serverName, string token)
    {
        return new PongCommand
        {
            Source = serverName,
            Token = token
        };
    }

    /// <summary>
    ///     Creates a PONG command from client to server
    /// </summary>
    public static PongCommand CreateFromClient(string token)
    {
        return new PongCommand
        {
            Token = token
        };
    }
}
