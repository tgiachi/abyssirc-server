using AbyssIrc.Protocol.Messages.Commands.Base;


namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
/// Represents the ERR_NOPRIVILEGES (481) error for unauthorized RESTART attempts
/// </summary>
public class ErrNoPrivilegesCommand : BaseIrcCommand
{
    /// <summary>
    /// The server name sending this error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client receiving this error
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The error message explaining lack of privileges
    /// </summary>
    public string ErrorMessage { get; set; } = "Permission denied - Insufficient privileges";

    public ErrNoPrivilegesCommand() : base("481")
    {
    }

    /// <summary>
    /// Parses the ERR_NOPRIVILEGES error message
    /// </summary>
    /// <param name="line">Raw IRC error message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 481 nickname :Permission denied

        // Reset existing data
        ServerName = null;
        Nickname = null;

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
        if (parts[0] != "481")
            return;

        // Extract nickname
        Nickname = parts[1];

        // Extract error message if present
        int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + 2);
        if (colonIndex != -1)
        {
            ErrorMessage = line.Substring(colonIndex + 1);
        }
    }

    /// <summary>
    /// Converts the error to its string representation
    /// </summary>
    /// <returns>Formatted error message</returns>
    public override string Write()
    {
        return string.IsNullOrEmpty(ServerName)
            ? $"481 {Nickname} :{ErrorMessage}"
            : $":{ServerName} 481 {Nickname} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_NOPRIVILEGES error
    /// </summary>
    /// <param name="serverName">Server sending the error</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="errorMessage">Optional custom error message</param>
    public static ErrNoPrivilegesCommand Create(
        string serverName,
        string nickname,
        string errorMessage = null
    )
    {
        return new ErrNoPrivilegesCommand
        {
            ServerName = serverName,
            Nickname = nickname,
            ErrorMessage = errorMessage ?? "Permission denied - Insufficient privileges"
        };
    }

    /// <summary>
    /// Generates an error response for unauthorized RESTART attempts
    /// </summary>
    /// <param name="serverName">Name of the server sending the error</param>
    /// <param name="nickname">Nickname of the user attempting restart</param>
    /// <returns>Error command indicating lack of privileges</returns>
    public static ErrNoPrivilegesCommand CreateNoPrivilegesError(string serverName, string nickname)
    {
        return new ErrNoPrivilegesCommand
        {
            ServerName = serverName,
            Nickname = nickname,
            ErrorMessage = "Permission denied - You must be an IRC operator to restart the server"
        };
    }
}
