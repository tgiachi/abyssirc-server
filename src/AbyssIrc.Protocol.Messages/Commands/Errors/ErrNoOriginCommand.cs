using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_NOORIGIN (409) error response
///     Returned when a client sends a command requiring an origin but doesn't provide one
/// </summary>
public class ErrNoOriginCommand : BaseIrcCommand
{
    public ErrNoOriginCommand() : base("409") => ErrorMessage = "No origin specified";

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_NOORIGIN format: ":server 409 nickname :No origin specified"

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
        // parts[1] should be "409"
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
        // Format: ":server 409 nickname :No origin specified"
        return $":{ServerName} 409 {Nickname} :{ErrorMessage}";
    }
}
