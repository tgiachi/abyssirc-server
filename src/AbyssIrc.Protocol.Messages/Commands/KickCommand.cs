using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

using System.Text;

/// <summary>
///     Represents an IRC KICK command used to remove users from a channel
/// </summary>
public class KickCommand : BaseIrcCommand
{
    public KickCommand() : base("KICK")
    {
    }

    /// <summary>
    ///     Source of the KICK command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     Channel from which the user is being kicked
    /// </summary>
    public string Channel { get; set; }

    /// <summary>
    ///     Nickname of the user being kicked
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    ///     Optional kick reason
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    ///     Parses a KICK command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Source = null;
        Channel = null;
        Target = null;
        Reason = null;

        // Check for source prefix
        if (line.StartsWith(':'))
        {
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                Source = line.Substring(1, spaceIndex - 1);
                line = line[(spaceIndex + 1)..].TrimStart();
            }
        }

        // Split remaining parts
        string[] parts = line.Split(' ');

        // First token should be "KICK"
        if (parts.Length == 0 || !parts[0].Equals("KICK", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Ensure we have a channel and target
        if (parts.Length < 3)
        {
            return;
        }

        // Set channel and target
        Channel = parts[1];
        Target = parts[2];

        // Extract reason if present (could start with a colon)
        if (parts.Length > 3)
        {
            // Check if reason starts with a colon
            if (parts[3].StartsWith(':'))
            {
                var reasonBuilder = new StringBuilder();
                reasonBuilder.Append(parts[3].Substring(1));

                // Add any remaining parts to the reason
                for (int i = 4; i < parts.Length; i++)
                {
                    reasonBuilder.Append(' ').Append(parts[i]);
                }

                Reason = reasonBuilder.ToString();
            }
            else
            {
                // Reason doesn't start with a colon
                var reasonBuilder = new StringBuilder();
                reasonBuilder.Append(parts[3]);

                // Add any remaining parts to the reason
                for (int i = 4; i < parts.Length; i++)
                {
                    reasonBuilder.Append(' ').Append(parts[i]);
                }

                Reason = reasonBuilder.ToString();
            }
        }
    }

    /// <summary>
    ///     Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted KICK command string</returns>
    public override string Write()
    {
        var commandBuilder = new StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add KICK, channel and target
        commandBuilder.Append("KICK ").Append(Channel).Append(' ').Append(Target);

        // Add reason if present
        if (!string.IsNullOrEmpty(Reason))
        {
            commandBuilder.Append(" :").Append(Reason);
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    ///     Creates a KICK command from a server or user
    /// </summary>
    /// <param name="source">Source of the kick (typically server name or user mask)</param>
    /// <param name="channel">Channel from which the user is being kicked</param>
    /// <param name="target">Nickname of the user being kicked</param>
    /// <param name="reason">Optional kick reason</param>
    /// <returns>A configured KICK command</returns>
    public static KickCommand Create(string source, string channel, string target, string reason = null)
    {
        return new KickCommand
        {
            Source = source,
            Channel = channel,
            Target = target,
            Reason = reason
        };
    }

    /// <summary>
    ///     Creates a KICK command from a client
    /// </summary>
    /// <param name="channel">Channel from which the user is being kicked</param>
    /// <param name="target">Nickname of the user being kicked</param>
    /// <param name="reason">Optional kick reason</param>
    /// <returns>A configured KICK command without a source</returns>
    public static KickCommand CreateFromClient(string channel, string target, string reason = null)
    {
        return new KickCommand
        {
            Channel = channel,
            Target = target,
            Reason = reason
        };
    }
}
