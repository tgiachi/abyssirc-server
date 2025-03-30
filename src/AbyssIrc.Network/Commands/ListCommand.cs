using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents the IRC LIST command used to list channels
/// </summary>
public class ListCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the LIST command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// List of channels to query (empty list means all channels)
    /// </summary>
    public List<string> Channels { get; set; } = new List<string>();

    /// <summary>
    /// Optional target server to query
    /// </summary>
    public string Target { get; set; }

    public ListCommand() : base("LIST")
    {
    }

    /// <summary>
    /// Parses a LIST command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Source = null;
        Channels.Clear();
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

        // First token should be "LIST"
        if (parts.Length == 0 || parts[0].ToUpper() != "LIST")
            return;

        // Check for channels
        if (parts.Length > 1)
        {
            // Split channels or check for target server
            var channelPart = parts[1];

            // Check if this might be a target server
            if (channelPart.Contains('.'))
            {
                Target = channelPart;

                // Check for channels after target
                if (parts.Length > 2)
                {
                    Channels.AddRange(parts[2].Split(','));
                }
            }
            else
            {
                // Parse channels
                Channels.AddRange(channelPart.Split(','));
            }
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted LIST command string</returns>
    public override string Write()
    {
        // Prepare base command
        var commandBuilder = new System.Text.StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add LIST command
        commandBuilder.Append("LIST");

        // Add target if present
        if (!string.IsNullOrEmpty(Target))
        {
            commandBuilder.Append(' ').Append(Target);
        }

        // Add channels if present
        if (Channels.Any())
        {
            commandBuilder.Append(' ').Append(string.Join(",", Channels));
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    /// Creates a LIST command to list all channels
    /// </summary>
    public static ListCommand Create()
    {
        return new ListCommand();
    }

    /// <summary>
    /// Creates a LIST command for specific channels
    /// </summary>
    /// <param name="channels">Channels to list</param>
    public static ListCommand Create(params string[] channels)
    {
        return new ListCommand
        {
            Channels = channels.ToList()
        };
    }

    /// <summary>
    /// Creates a LIST command for a specific target server
    /// </summary>
    /// <param name="target">Target server to query</param>
    /// <param name="channels">Optional channels to list</param>
    public static ListCommand CreateForTarget(string target, params string[] channels)
    {
        return new ListCommand
        {
            Target = target,
            Channels = channels.ToList()
        };
    }
}
