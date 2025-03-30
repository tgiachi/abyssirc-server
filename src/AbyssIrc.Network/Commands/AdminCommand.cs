using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents the IRC ADMIN command used to query administrative information about the server
/// </summary>
public class AdminCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the ADMIN command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Optional target server to query administrative information from
    /// </summary>
    public string Target { get; set; }

    public AdminCommand() : base("ADMIN")
    {
    }

    /// <summary>
    /// Parses an ADMIN command from a raw IRC message
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

        // First token should be "ADMIN"
        if (parts.Length == 0 || parts[0].ToUpper() != "ADMIN")
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
    /// <returns>Formatted ADMIN command string</returns>
    public override string Write()
    {
        // Prepare base command
        var commandBuilder = new System.Text.StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add ADMIN command
        commandBuilder.Append("ADMIN");

        // Add target if present
        if (!string.IsNullOrEmpty(Target))
        {
            commandBuilder.Append(' ').Append(Target);
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    /// Creates an ADMIN command to query server administrative information
    /// </summary>
    public static AdminCommand Create()
    {
        return new AdminCommand();
    }

    /// <summary>
    /// Creates an ADMIN command for a specific target server
    /// </summary>
    /// <param name="target">Target server to query</param>
    public static AdminCommand CreateForTarget(string target)
    {
        return new AdminCommand
        {
            Target = target
        };
    }
}
