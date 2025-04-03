using System.Text;
using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC TOPIC command for setting or querying channel topics
/// </summary>
public class TopicCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the TOPIC command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Channel to query or set topic for
    /// </summary>
    public string Channel { get; set; }

    /// <summary>
    /// New topic to set (optional)
    /// </summary>
    public string Topic { get; set; }

    public TopicCommand() : base("TOPIC")
    {
    }

    /// <summary>
    /// Parses a TOPIC command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Source = null;
        Channel = null;
        Topic = null;

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

        // First token should be "TOPIC"
        if (parts.Length == 0 || parts[0].ToUpper() != "TOPIC")
            return;

        // Check for channel
        if (parts.Length >= 2)
        {
            Channel = parts[1];

            // Check for topic (starts with ':')
            int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + 2);
            if (colonIndex != -1)
            {
                Topic = line.Substring(colonIndex + 1);
            }
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted TOPIC command string</returns>
    public override string Write()
    {
        // Prepare base command
        StringBuilder commandBuilder = new StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add TOPIC and channel
        commandBuilder.Append("TOPIC ").Append(Channel);

        // Add topic if present
        if (!string.IsNullOrEmpty(Topic))
        {
            commandBuilder.Append(" :").Append(Topic);
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    /// Creates a TOPIC command to query a channel's topic
    /// </summary>
    /// <param name="channel">Channel to query</param>
    public static TopicCommand Create(string channel)
    {
        return new TopicCommand
        {
            Channel = channel
        };
    }

    /// <summary>
    /// Creates a TOPIC command to set a channel's topic
    /// </summary>
    /// <param name="channel">Channel to set topic for</param>
    /// <param name="topic">New topic</param>
    public static TopicCommand CreateWithTopic(string channel, string topic)
    {
        return new TopicCommand
        {
            Channel = channel,
            Topic = topic
        };
    }
}
