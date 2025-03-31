using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
/// Represents an IRC ERR_NOPRIVILEGES (481) error response
/// Returned when a user tries to perform an operation that requires server operator privileges
/// </summary>
public class ErrNoPrivileges : BaseIrcCommand
{
    public ErrNoPrivileges() : base("481") => ErrorMessage = "Permission denied - You're not an IRC operator";

    /// <summary>
    /// The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The error message explaining the lack of privileges
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Optional description of the operation that was attempted
    /// </summary>
    public string OperationDescription { get; set; }

    public override void Parse(string line)
    {
        // ERR_NOPRIVILEGES format: ":server 481 nickname :Permission denied"

        if (!line.StartsWith(':'))
        {
            return; // Invalid format for server response
        }

        var parts = line.Split(' ', 4); // Maximum of 4 parts

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "481"
        Nickname = parts[2];

        // Extract the error message (removes the leading ":")
        if (parts[3].StartsWith(":"))
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
        // Format: ":server 481 nickname :Permission denied"
        return $":{ServerName} 481 {Nickname} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_NOPRIVILEGES (481) reply
    /// </summary>
    /// <param name="serverName">Name of the server sending the error</param>
    /// <param name="nickname">Nickname of the user receiving the error</param>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <param name="operationDescription">Description of the attempted operation (optional)</param>
    public static ErrNoPrivileges Create(
        string serverName,
        string nickname,
        string errorMessage = null,
        string operationDescription = null
    )
    {
        return new ErrNoPrivileges
        {
            ServerName = serverName,
            Nickname = nickname,
            ErrorMessage = errorMessage ?? "Permission denied - You're not an IRC operator",
            OperationDescription = operationDescription
        };
    }
}
