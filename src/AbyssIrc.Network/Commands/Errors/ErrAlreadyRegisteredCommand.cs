using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
/// Represents ERR_ALREADYREGISTERED (462) error
/// Sent when a client tries to register after already being registered
/// </summary>
public class ErrAlreadyRegisteredCommand : BaseIrcCommand
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
    /// The error message explaining the registration issue
    /// </summary>
    public string ErrorMessage { get; set; } = "You may not reregister";

    public ErrAlreadyRegisteredCommand() : base("462")
    {
    }

    /// <summary>
    /// Parses the ERR_ALREADYREGISTERED error message
    /// </summary>
    /// <param name="line">Raw IRC error message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 462 nickname :You may not reregister

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
        if (parts[0] != "462")
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
            ? $"462 {Nickname} :{ErrorMessage}"
            : $":{ServerName} 462 {Nickname} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_ALREADYREGISTERED error
    /// </summary>
    /// <param name="serverName">Server sending the error</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="errorMessage">Optional custom error message</param>
    public static ErrAlreadyRegisteredCommand Create(
        string serverName,
        string nickname,
        string errorMessage = null
    )
    {
        return new ErrAlreadyRegisteredCommand
        {
            ServerName = serverName,
            Nickname = nickname,
            ErrorMessage = errorMessage ?? "You may not reregister"
        };
    }
}
