using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_NICKCOLLISION (436) error response
///     Returned when a NICK command cannot be processed because the nickname is already in use on the network
/// </summary>
public class ErrNickCollisionCommand : BaseIrcCommand
{
    public ErrNickCollisionCommand() : base("436") => ErrorMessage = "Nickname collision KILL";

    /// <summary>
    ///     The nickname that caused the collision
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The error message explaining the collision
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_NICKCOLLISION format: ":server 436 * nickname :Nickname collision KILL"

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
        // parts[1] should be "436"
        // parts[2] is typically "*" or the target (we don't need to store this)

        Nickname = parts[3];

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
        // Format: ":server 436 * nickname :Error message"
        return $":{ServerName} 436 * {Nickname} :{ErrorMessage}";
    }
}
