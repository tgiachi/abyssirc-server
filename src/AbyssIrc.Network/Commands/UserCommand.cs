using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents an IRC USER command used during initial registration
/// </summary>
public class UserCommand : BaseIrcCommand
{
    /// <summary>
    /// The username specified by the client
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// The mode/flags for the user (usually 0 or 8)
    /// </summary>
    public string Mode { get; set; }

    /// <summary>
    /// The unused parameter (traditionally * or 0)
    /// </summary>
    public string Unused { get; set; }

    /// <summary>
    /// The real name/gecos field for the user
    /// </summary>
    public string RealName { get; set; }

    public UserCommand() : base("USER")
    {
    }

    public override void Parse(string line)
    {
        // Example: USER textual 0 * :Textual User

        // Split into parts
        var parts = line.Split(' ');

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        // parts[0] should be "USER"
        Username = parts[1];
        Mode = parts[2];
        Unused = parts[3];

        // The real name is everything after the : character
        int colonPos = line.IndexOf(':', parts[0].Length);
        RealName = colonPos != -1
            ? line[(colonPos + 1)..]
            :
            // Fallback if no colon is found (shouldn't happen in well-formed messages)
            parts[4];
    }

    public override string Write()
    {
        return $"USER {Username} {Mode} {Unused} :{RealName}";
    }
}
