using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents an IRC ISON command used to check if specific users are online
/// </summary>
public class IsonCommand : BaseIrcCommand
{
    public IsonCommand() : base("ISON")
    {
    }

    /// <summary>
    ///     Source of the command (typically empty for client-originated queries)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     List of nicknames to check online status for
    /// </summary>
    public List<string> Nicknames { get; set; } = new();

    public override void Parse(string line)
    {
        // Format: [:<source>] ISON <nickname> [<nickname> ...]

        // Handle source prefix if present
        if (line.StartsWith(':'))
        {
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex == -1)
            {
                return; // Invalid format
            }

            Source = line.Substring(1, spaceIndex - 1);
            line = line.Substring(spaceIndex + 1).TrimStart();
        }

        // Split into tokens
        var tokens = line.Split(' ');

        // First token should be "ISON"
        if (tokens.Length == 0 || tokens[0].ToUpper() != "ISON")
        {
            return;
        }

        // Remaining tokens are nicknames
        for (var i = 1; i < tokens.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(tokens[i]))
            {
                Nicknames.Add(tokens[i]);
            }
        }
    }

    public override string Write()
    {
        var prefix = string.IsNullOrWhiteSpace(Source) ? "" : $":{Source} ";
        var nicknames = string.Join(" ", Nicknames);

        return $"{prefix}ISON {nicknames}";
    }
}
