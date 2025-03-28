using AbyssIrc.Network.Commands.Base;
using System.Collections.Generic;
using System.Linq;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents an IRC ISON command used to check if specific users are online
/// </summary>
public class IsonCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the command (typically empty for client-originated queries)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// List of nicknames to check online status for
    /// </summary>
    public List<string> Nicknames { get; set; } = new List<string>();

    public IsonCommand() : base("ISON")
    {
    }

    public override void Parse(string line)
    {
        // Format: [:<source>] ISON <nickname> [<nickname> ...]

        // Handle source prefix if present
        if (line.StartsWith(':'))
        {
            int spaceIndex = line.IndexOf(' ');
            if (spaceIndex == -1)
                return; // Invalid format

            Source = line.Substring(1, spaceIndex - 1);
            line = line.Substring(spaceIndex + 1).TrimStart();
        }

        // Split into tokens
        string[] tokens = line.Split(' ');

        // First token should be "ISON"
        if (tokens.Length == 0 || tokens[0].ToUpper() != "ISON")
            return;

        // Remaining tokens are nicknames
        for (int i = 1; i < tokens.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(tokens[i]))
            {
                Nicknames.Add(tokens[i]);
            }
        }
    }

    public override string Write()
    {
        string prefix = string.IsNullOrWhiteSpace(Source) ? "" : $":{Source} ";
        string nicknames = string.Join(" ", Nicknames);

        return $"{prefix}ISON {nicknames}";
    }
}
