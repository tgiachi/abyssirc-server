using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents an IRC TIME command to query server time
/// </summary>
public class TimeCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the TIME command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Optional target server to query time from
    /// </summary>
    public string Target { get; set; }

    public TimeCommand() : base("TIME")
    {
    }

    /// <summary>
    /// Parses a TIME command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Source = null;
        Target = null;

        // Check for source prefix
        if (line.StartsWith(':'))
        {
            int spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                Source = line.Substring(1, spaceIndex - 1);
                line = line.Substring(spaceIndex + 1).TrimStart();
            }
        }

        // Split remaining parts
        string[] parts = line.Split(' ');

        // First token should be "TIME"
        if (parts.Length == 0 || !parts[0].Equals("TIME", StringComparison.CurrentCultureIgnoreCase))
        {
            return;
        }

        // Check for optional target server
        if (parts.Length > 1)
        {
            Target = parts[1];
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted TIME command string</returns>
    public override string Write()
    {
        // Prepare base command
        var commandBuilder = new System.Text.StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add TIME command
        commandBuilder.Append("TIME");

        // Add target if present
        if (!string.IsNullOrEmpty(Target))
        {
            commandBuilder.Append(' ').Append(Target);
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    /// Creates a TIME command to query server time
    /// </summary>
    /// <param name="targetServer">Optional target server to query</param>
    public static TimeCommand Create(string targetServer = null)
    {
        return new TimeCommand
        {
            Target = targetServer
        };
    }
}
