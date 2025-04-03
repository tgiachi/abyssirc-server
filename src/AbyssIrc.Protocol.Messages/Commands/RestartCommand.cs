using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC RESTART command used by operators to restart the server
/// </summary>
public class RestartCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the RESTART command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Optional restart reason or comment
    /// </summary>
    public string Reason { get; set; }

    public RestartCommand() : base("RESTART")
    {
    }

    /// <summary>
    /// Parses a RESTART command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Source = null;
        Reason = null;

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

        // First token should be "RESTART"
        if (parts.Length == 0 || parts[0].ToUpper() != "RESTART")
            return;

        // Check for optional restart reason
        int colonIndex = line.IndexOf(':', parts[0].Length + 1);
        if (colonIndex != -1)
        {
            Reason = line.Substring(colonIndex + 1).Trim();
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted RESTART command string</returns>
    public override string Write()
    {
        // Prepare base command
        var commandBuilder = new System.Text.StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add RESTART command
        commandBuilder.Append("RESTART");

        // Add reason if present
        if (!string.IsNullOrEmpty(Reason))
        {
            commandBuilder.Append(" :").Append(Reason);
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    /// Creates a RESTART command
    /// </summary>
    /// <param name="reason">Optional restart reason</param>
    public static RestartCommand Create(string reason = null)
    {
        return new RestartCommand
        {
            Reason = reason
        };
    }
}
