using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_UNKNOWNCOMMAND (421) error response
///     Returned when a client sends a command that the server doesn't recognize
/// </summary>
public class ErrUnknownCommandCommand : BaseIrcCommand
{
    public ErrUnknownCommandCommand() : base("421") => ErrorMessage = "Unknown command";

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The unrecognized command
    /// </summary>
    public string CommandName { get; set; }

    /// <summary>
    ///     The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_UNKNOWNCOMMAND format: ":server 421 nickname command :Unknown command"

        if (!line.StartsWith(":"))
        {
            return; // Invalid format for server response
        }

        var parts = line.Split(' ', 5); // Maximum of 5 parts

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "421"
        Nickname = parts[2];
        CommandName = parts[3];

        // Extract the error message (removes the leading ":")
        if (parts[4].StartsWith(":"))
        {
            ErrorMessage = parts[4].Substring(1);
        }
        else
        {
            ErrorMessage = parts[4];
        }
    }

    public override string Write()
    {
        // Format: ":server 421 nickname command :Unknown command"
        return $":{ServerName} 421 {Nickname} {CommandName} :{ErrorMessage}";
    }
}
