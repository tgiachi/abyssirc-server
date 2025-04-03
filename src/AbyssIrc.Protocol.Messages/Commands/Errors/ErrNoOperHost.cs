using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
/// Represents an IRC ERR_NOOPERHOST (491) error response
/// Returned when a client attempts to use OPER command from a non-authorized host
/// Format: ":server 491 nickname :No O-lines for your host"
/// </summary>
public class ErrNoOperHost : BaseIrcCommand
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
    public string ErrorMessage { get; set; } = "No O-lines for your host";

    public ErrNoOperHost() : base("491")
    {
    }

    public override void Parse(string line)
    {
        // ERR_NOOPERHOST format: ":server 491 nickname :No O-lines for your host"

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
        // parts[1] should be "491"
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
        // Format: ":server 491 nickname :No O-lines for your host"
        return $":{ServerName} 491 {Nickname} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_NOOPERHOST error response
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>A formatted ERR_NOOPERHOST error</returns>
    public static ErrNoOperHost Create(
        string serverName,
        string nickname,
        string errorMessage = null)
    {
        return new ErrNoOperHost
        {
            ServerName = serverName,
            Nickname = nickname,
            ErrorMessage = errorMessage ?? "No O-lines for your host"
        };
    }
}
