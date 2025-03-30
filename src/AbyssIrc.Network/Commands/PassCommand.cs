using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents the IRC PASS command used during connection registration
/// </summary>
public class PassCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the PASS command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The password provided in the PASS command
    /// </summary>
    public string Password { get; set; }

    public PassCommand() : base("PASS")
    {
    }

    /// <summary>
    /// Parses a PASS command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Source = null;
        Password = null;

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

        // First token should be "PASS"
        if (parts.Length == 0 || parts[0].ToUpper() != "PASS")
            return;

        // Extract password
        if (parts.Length > 1)
        {
            // Check if password starts with a colon (for passwords with spaces)
            int colonIndex = line.IndexOf(':', parts[0].Length + 1);
            if (colonIndex != -1)
            {
                Password = line.Substring(colonIndex + 1);
            }
            else
            {
                Password = parts[1];
            }
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted PASS command string</returns>
    public override string Write()
    {
        // Prepare base command
        var commandBuilder = new System.Text.StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add PASS command
        commandBuilder.Append("PASS");

        // Add password
        if (!string.IsNullOrEmpty(Password))
        {
            // Use colon if password might contain spaces
            commandBuilder.Append(" :").Append(Password);
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    /// Creates a PASS command
    /// </summary>
    /// <param name="password">Server connection password</param>
    public static PassCommand Create(string password)
    {
        return new PassCommand
        {
            Password = password
        };
    }
}
