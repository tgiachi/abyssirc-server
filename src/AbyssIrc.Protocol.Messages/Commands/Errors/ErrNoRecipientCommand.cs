using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_NORECIPIENT (411) error response
///     Returned when a client sends a command without specifying a recipient
/// </summary>
public class ErrNoRecipientCommand : BaseIrcCommand
{
    public ErrNoRecipientCommand() : base("411")
    {
    }

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The command that was missing a recipient
    /// </summary>
    public string CommandName { get; set; }

    /// <summary>
    ///     The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_NORECIPIENT format: ":server 411 nickname :No recipient given (COMMAND)"

        if (!line.StartsWith(":"))
        {
            return; // Invalid format for server response
        }

        var parts = line.Split(' ', 4); // Maximum of 4 parts

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "411"
        Nickname = parts[2];

        // Extract the error message (removes the leading ":")
        var fullMessage = parts[3].StartsWith(":") ? parts[3].Substring(1) : parts[3];

        // Parse out the command name from the message format "No recipient given (COMMAND)"
        var startIndex = fullMessage.LastIndexOf('(');
        var endIndex = fullMessage.LastIndexOf(')');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            CommandName = fullMessage.Substring(startIndex + 1, endIndex - startIndex - 1);
            ErrorMessage = "No recipient given";
        }
        else
        {
            // If we can't parse it properly, just store the full message
            ErrorMessage = fullMessage;
        }
    }

    public override string Write()
    {
        // Format: ":server 411 nickname :No recipient given (COMMAND)"
        return $":{ServerName} 411 {Nickname} :{ErrorMessage} ({CommandName})";
    }
}
