using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
/// Represents the ERR_NEEDMOREPARAMS (461) numeric reply
/// Sent when a command is missing required parameters
/// </summary>
public class ErrNeedMoreParamsCommand : BaseIrcCommand
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
    /// The command that lacks required parameters
    /// </summary>
    public string CommandName { get; set; }

    /// <summary>
    /// The error message explaining the lack of parameters
    /// </summary>
    public string ErrorMessage { get; set; } = "Not enough parameters";

    public ErrNeedMoreParamsCommand() : base("461")
    {
    }

    /// <summary>
    /// Parses the ERR_NEEDMOREPARAMS error message
    /// </summary>
    /// <param name="line">Raw IRC error message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 461 nickname COMMAND :Not enough parameters

        // Reset existing data
        ServerName = null;
        Nickname = null;
        CommandName = null;

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
        if (parts.Length < 3)
            return;

        // Verify the numeric code
        if (parts[0] != "461")
            return;

        // Extract nickname
        Nickname = parts[1];

        // Extract command name
        CommandName = parts[2];

        // Extract error message if present
        int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + parts[2].Length + 3);
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
        // Prepare error message
        return string.IsNullOrEmpty(ServerName)
            ? $"461 {Nickname} {CommandName} :{ErrorMessage}"
            : $":{ServerName} 461 {Nickname} {CommandName} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_NEEDMOREPARAMS error
    /// </summary>
    /// <param name="serverName">Server sending the error</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="commandName">Command missing parameters</param>
    /// <param name="errorMessage">Optional custom error message</param>
    public static ErrNeedMoreParamsCommand Create(
        string serverName,
        string nickname,
        string commandName,
        string errorMessage = null
    )
    {
        return new ErrNeedMoreParamsCommand
        {
            ServerName = serverName,
            Nickname = nickname,
            CommandName = commandName,
            ErrorMessage = errorMessage ?? "Not enough parameters"
        };
    }
}
