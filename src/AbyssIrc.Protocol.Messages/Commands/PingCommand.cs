using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC PING command used for connection monitoring
/// </summary>
public class PingCommand : BaseIrcCommand
{
    /// <summary>
    /// The token/parameter included in the PING
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// The source of the PING (typically server name if sent by server)
    /// </summary>
    public string Source { get; set; }

    public PingCommand() : base("PING")
    {
    }


    public PingCommand(string source, string token) : base("PING")
    {
        Source = source;
        Token = token;
    }

    public override void Parse(string line)
    {
        // Examples:
        // Client to server: PING :token
        // Server to client: :server.com PING :token

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
            return $":{Source} PING :{Token}";
        }
        else
        {
            return $"PING :{Token}";
        }
    }

    /// <summary>
    /// Creates a PING command from server to client
    /// </summary>
    public static PingCommand CreateFromServer(string serverName, string token)
    {
        return new PingCommand
        {
            Source = serverName,
            Token = token
        };
    }

    /// <summary>
    /// Creates a PING command from client to server
    /// </summary>
    public static PingCommand CreateFromClient(string token)
    {
        return new PingCommand
        {
            Token = token
        };
    }
}
