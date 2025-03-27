using System.Text;
using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents an IRC MODE command used for changing user and channel modes
/// </summary>
public class ModeCommand : BaseIrcCommand
{
    /// <summary>
    /// The target of the mode change (nickname or channel)
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// The mode string (e.g. "+o-i")
    /// </summary>
    public string ModeString { get; set; }

    /// <summary>
    /// Optional parameters for modes that require them
    /// </summary>
    public List<string> Parameters { get; set; } = new List<string>();

    /// <summary>
    /// The source of the mode change (typically a user prefix)
    /// </summary>
    public string Source { get; set; }

    public ModeCommand() : base("MODE")
    {
    }

    public override void Parse(string line)
    {
        // Examples:
        // MODE nickname +i
        // MODE #channel +o nickname
        // :nick!user@host MODE #channel +v otheruser

        var parts = line.Split(' ');

        if (parts[0].StartsWith(":"))
        {
            // Has a source
            Source = parts[0].TrimStart(':');

            if (parts.Length > 2)
            {
                Target = parts[2];

                if (parts.Length > 3)
                {
                    ModeString = parts[3];

                    // Collect any parameters
                    for (int i = 4; i < parts.Length; i++)
                    {
                        Parameters.Add(parts[i]);
                    }
                }
            }
        }
        else
        {
            // No source
            if (parts.Length > 1)
            {
                Target = parts[1];

                if (parts.Length > 2)
                {
                    ModeString = parts[2];

                    // Collect any parameters
                    for (int i = 3; i < parts.Length; i++)
                    {
                        Parameters.Add(parts[i]);
                    }
                }
            }
        }
    }

    public override string Write()
    {
        var result = new StringBuilder();

        if (!string.IsNullOrEmpty(Source))
        {
            result.Append($":{Source} ");
        }

        result.Append($"MODE {Target} {ModeString}");

        foreach (var param in Parameters)
        {
            result.Append($" {param}");
        }

        return result.ToString();
    }
}
