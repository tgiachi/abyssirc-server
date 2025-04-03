using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_YOUREBANNEDCREEP (465) error response
///     Returned when a client attempts to connect but is banned from the server
/// </summary>
public class ErrYoureBannedCreepCommand : BaseIrcCommand
{
    public ErrYoureBannedCreepCommand() : base("465") => ErrorMessage = "You are banned from this server";

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname or * for a pre-registration client
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The error message explaining the ban
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_YOUREBANNEDCREEP format: ":server 465 nickname :You are banned from this server"

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
        // parts[1] should be "465"
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
        // Format: ":server 465 nickname :You are banned from this server"
        return $":{ServerName} 465 {Nickname} :{ErrorMessage}";
    }
}
