using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC WHO command used to query information about users
/// Format: "WHO [mask] [o]" or ":source WHO [mask] [o]"
/// </summary>
public class WhoCommand : BaseIrcCommand
{
    /// <summary>
    /// The source of the command (for server-to-server communication)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Mask to filter results (can be a channel name, a nickname pattern, or a host pattern)
    /// </summary>
    public string Mask { get; set; }

    /// <summary>
    /// If true ('o' parameter is present), only show operators
    /// </summary>
    public bool OnlyOperators { get; set; }

    public WhoCommand() : base("WHO")
    {
    }

    public override void Parse(string line)
    {
        // Handle source prefix if present
        string parseLine = line;
        if (line.StartsWith(':'))
        {
            int spaceIndex = line.IndexOf(' ');
            if (spaceIndex == -1)
                return; // Invalid format

            Source = line.Substring(1, spaceIndex - 1);
            parseLine = line.Substring(spaceIndex + 1).TrimStart();
        }

        // Split into parts
        var parts = parseLine.Split(' ');

        // First token should be "WHO"
        if (parts.Length == 0 || parts[0].ToUpper() != "WHO")
            return;

        // Parse mask if present
        if (parts.Length > 1 && !parts[1].Equals("o", StringComparison.OrdinalIgnoreCase))
        {
            Mask = parts[1];
        }

        // Check for the 'o' parameter for operators only
        if ((parts.Length > 1 && parts[1].Equals("o", StringComparison.OrdinalIgnoreCase)) ||
            (parts.Length > 2 && parts[2].Equals("o", StringComparison.OrdinalIgnoreCase)))
        {
            OnlyOperators = true;
        }
    }

    public override string Write()
    {
        string command = "WHO";

        // Add mask if present
        if (!string.IsNullOrEmpty(Mask))
        {
            command += $" {Mask}";
        }

        // Add 'o' flag if needed
        if (OnlyOperators)
        {
            command += " o";
        }

        // Add source prefix if needed
        if (!string.IsNullOrEmpty(Source))
        {
            return $":{Source} {command}";
        }

        return command;
    }

    /// <summary>
    /// Creates a WHO command to query information about users
    /// </summary>
    /// <param name="mask">Optional mask to filter results (channel name, nickname pattern, or host pattern)</param>
    /// <param name="onlyOperators">If true, only show IRC operators</param>
    /// <returns>A properly formatted WHO command</returns>
    public static WhoCommand Create(string mask = null, bool onlyOperators = false)
    {
        return new WhoCommand
        {
            Mask = mask,
            OnlyOperators = onlyOperators
        };
    }

    /// <summary>
    /// Creates a WHO command with source (typically for server-to-server communication)
    /// </summary>
    /// <param name="source">Source of the command</param>
    /// <param name="mask">Optional mask to filter results</param>
    /// <param name="onlyOperators">If true, only show IRC operators</param>
    /// <returns>A properly formatted WHO command with source</returns>
    public static WhoCommand CreateWithSource(string source, string mask = null, bool onlyOperators = false)
    {
        return new WhoCommand
        {
            Source = source,
            Mask = mask,
            OnlyOperators = onlyOperators
        };
    }
}
