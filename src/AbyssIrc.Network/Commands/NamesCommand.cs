using System.Text;
using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents an IRC NAMES command for querying channel users
/// </summary>
public class NamesCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the NAMES command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// List of channels to query
    /// </summary>
    public List<string> Channels { get; set; } = new List<string>();

    /// <summary>
    /// Optional network/server name to query
    /// </summary>
    public string Network { get; set; }

    public NamesCommand() : base("NAMES")
    {
    }

    /// <summary>
    /// Parses a NAMES command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Channels.Clear();
        Source = null;
        Network = null;

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

        // First token should be "NAMES"
        if (parts.Length == 0 || parts[0].ToUpper() != "NAMES")
            return;

        // Check for channels or network
        if (parts.Length >= 2)
        {
            // Check if second part could be a network name
            if (parts.Length >= 3 && parts[1].Contains('.'))
            {
                Network = parts[1];
                // Channels would be in the third part
                Channels.AddRange(parts[2].Split(','));
            }
            else
            {
                // Split channels
                Channels.AddRange(parts[1].Split(','));
            }
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted NAMES command string</returns>
    public override string Write()
    {
        // Prepare base command
        StringBuilder commandBuilder = new StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add NAMES command
        commandBuilder.Append("NAMES");

        // Add network if present
        if (!string.IsNullOrEmpty(Network))
        {
            commandBuilder.Append(' ').Append(Network);
        }

        // Add channels if present
        if (Channels.Any())
        {
            commandBuilder.Append(' ').Append(string.Join(",", Channels));
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    /// Creates a NAMES command to query users in specified channels
    /// </summary>
    /// <param name="channels">Channels to query</param>
    public static NamesCommand Create(params string[] channels)
    {
        return new NamesCommand
        {
            Channels = channels.ToList()
        };
    }
}
