using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_WASNOSUCHNICK (406) error response
///     Returned when a client tries to perform an operation on a nickname that doesn't exist anymore
/// </summary>
public class ErrWasNoSuchNickCommand : BaseIrcCommand
{
    public ErrWasNoSuchNickCommand() : base("406") => ErrorMessage = "There was no such nickname";

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The nickname that doesn't exist anymore
    /// </summary>
    public string TargetNick { get; set; }

    /// <summary>
    ///     The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_WASNOSUCHNICK format: ":server 406 nickname target :There was no such nickname"

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
        // parts[1] should be "406"
        Nickname = parts[2];
        TargetNick = parts[3];

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
        // Format: ":server 406 nickname target :There was no such nickname"
        return $":{ServerName} 406 {Nickname} {TargetNick} :{ErrorMessage}";
    }
}
