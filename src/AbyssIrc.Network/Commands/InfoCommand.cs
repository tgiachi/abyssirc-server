using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents the IRC INFO command used to query server information
/// </summary>
public class InfoCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the INFO command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Optional target server to query information from
    /// </summary>
    public string Target { get; set; }

    public InfoCommand() : base("INFO")
    {
    }

    /// <summary>
    /// Parses an INFO command from a raw IRC message
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

        // First token should be "INFO"
        if (parts.Length == 0 || parts[0].ToUpper() != "INFO")
            return;

        // Check for optional target server
        if (parts.Length > 1)
        {
            Target = parts[1];
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted INFO command string</returns>
    public override string Write()
    {
        // Prepare base command
        var commandBuilder = new System.Text.StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add INFO command
        commandBuilder.Append("INFO");

        // Add target if present
        if (!string.IsNullOrEmpty(Target))
        {
            commandBuilder.Append(' ').Append(Target);
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    /// Creates an INFO command to query server information
    /// </summary>
    public static InfoCommand Create()
    {
        return new InfoCommand();
    }

    /// <summary>
    /// Creates an INFO command for a specific target server
    /// </summary>
    /// <param name="target">Target server to query</param>
    public static InfoCommand CreateForTarget(string target)
    {
        return new InfoCommand
        {
            Target = target
        };
    }
}
