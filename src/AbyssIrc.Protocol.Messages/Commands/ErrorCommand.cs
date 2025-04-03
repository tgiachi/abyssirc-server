using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC ERROR command used to indicate error conditions and connection termination
/// </summary>
public class ErrorCommand : BaseIrcCommand
{
    /// <summary>
    /// The error message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The source of the error (typically server name)
    /// </summary>
    public string Source { get; set; }

    public ErrorCommand() : base("ERROR")
    {
    }

    public override void Parse(string line)
    {
        // Examples:
        // ERROR :Closing Link: nickname[host] (Ping timeout: 180 seconds)
        // :server.com ERROR :Server shutting down

        var parts = line.Split(' ', 2);

        if (parts[0].StartsWith(":"))
        {
            // Server prefixed format
            Source = parts[0].TrimStart(':');

            if (parts.Length > 1 && parts[1].StartsWith("ERROR"))
            {
                int colonPos = line.IndexOf(':', parts[0].Length);
                if (colonPos != -1)
                {
                    Message = line.Substring(colonPos + 1);
                }
            }
        }
        else
        {
            // Simple format
            // parts[0] should be "ERROR"

            if (parts.Length > 1)
            {
                if (parts[1].StartsWith(":"))
                {
                    Message = parts[1].Substring(1);
                }
                else
                {
                    Message = parts[1];
                }
            }
        }
    }

    public override string Write()
    {
        if (!string.IsNullOrEmpty(Source))
        {
            return $":{Source} ERROR :{Message}";
        }
        else
        {
            return $"ERROR :{Message}";
        }
    }

    /// <summary>
    /// Creates an ERROR command with a server source
    /// </summary>
    public static ErrorCommand CreateFromServer(string serverName, string message)
    {
        return new ErrorCommand
        {
            Source = serverName,
            Message = message
        };
    }

    /// <summary>
    /// Creates a standard ping timeout ERROR message
    /// </summary>
    public static ErrorCommand CreatePingTimeout(string serverName, string nickname, string hostname, int timeoutSeconds)
    {
        string message = $"Closing Link: {nickname}[{hostname}] (Ping timeout: {timeoutSeconds} seconds)";
        return new ErrorCommand
        {
            Source = serverName,
            Message = message
        };
    }

    /// <summary>
    /// Creates a standard quit ERROR message
    /// </summary>
    public static ErrorCommand CreateQuit(string serverName, string nickname, string hostname, string quitMessage)
    {
        string message = $"Closing Link: {nickname}[{hostname}] (Quit: {quitMessage})";
        return new ErrorCommand
        {
            Source = serverName,
            Message = message
        };
    }
}
