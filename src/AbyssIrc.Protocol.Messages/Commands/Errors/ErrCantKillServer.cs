using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
/// Represents an IRC ERR_CANTKILLSERVER (483) error response
/// Returned when a client tries to KILL a server connection or service
/// Format: ":server 483 nickname :You can't kill a server!"
/// </summary>
public class ErrCantKillServer : BaseIrcCommand
{
    /// <summary>
    /// The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; } = "You can't kill a server!";

    public ErrCantKillServer() : base("483")
    {
    }

    public override void Parse(string line)
    {
        // ERR_CANTKILLSERVER format: ":server 483 nickname :You can't kill a server!"

        if (!line.StartsWith(':'))
        {
            return; // Invalid format for server response
        }

        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "483"
        Nickname = parts[2];

        // Extract the error message (removes the leading ":")
        if (parts[3].StartsWith(':'))
        {
            ErrorMessage = parts[3].Substring(1);
        }
        else
        {
            ErrorMessage = parts[3];
        }
    }

    public override string Write()
    {
        // Format: ":server 483 nickname :You can't kill a server!"
        return $":{ServerName} 483 {Nickname} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_CANTKILLSERVER error response
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>A formatted ERR_CANTKILLSERVER error</returns>
    public static ErrCantKillServer Create(
        string serverName,
        string nickname,
        string errorMessage = null)
    {
        return new ErrCantKillServer
        {
            ServerName = serverName,
            Nickname = nickname,
            ErrorMessage = errorMessage ?? "You can't kill a server!"
        };
    }
}
