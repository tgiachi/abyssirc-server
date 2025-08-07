using System.Text;
using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents an IRC PART command for leaving channels
/// </summary>
public class PartCommand : BaseIrcCommand
{
    public PartCommand() : base("PART")
    {
    }

    /// <summary>
    ///     Source of the PART command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     List of channels to leave
    /// </summary>
    public List<string> Channels { get; set; } = new();

    /// <summary>
    ///     Part message (optional reason for leaving)
    /// </summary>
    public string PartMessage { get; set; }

    /// <summary>
    ///     Parses a PART command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Channels.Clear();
        Source = null;
        PartMessage = null;

        // Check for source prefix
        if (line.StartsWith(':'))
        {
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                Source = line.Substring(1, spaceIndex - 1);
                line = line.Substring(spaceIndex + 1).TrimStart();
            }
        }

        // Split remaining parts
        var parts = line.Split(' ');

        // First token should be "PART"
        if (parts.Length == 0 || parts[0].ToUpper() != "PART")
        {
            return;
        }

        // Check for channels
        if (parts.Length >= 2)
        {
            // Split channels
            var channels = parts[1].Split(',');
            Channels.AddRange(channels);

            // Check for part message
            var colonIndex = line.IndexOf(':', parts[0].Length);
            if (colonIndex != -1 && colonIndex > parts[0].Length + parts[1].Length + 1)
            {
                PartMessage = line.Substring(colonIndex + 1);
            }
        }
    }

    /// <summary>
    ///     Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted PART command string</returns>
    public override string Write()
    {
        // Prepare base command
        var commandBuilder = new StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add PART and channels
        commandBuilder.Append("PART ").Append(string.Join(",", Channels));

        // Add part message if present
        if (!string.IsNullOrEmpty(PartMessage))
        {
            commandBuilder.Append(" :").Append(PartMessage);
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    ///     Creates a PART command to leave specified channels
    /// </summary>
    /// <param name="channels">Channels to leave</param>
    public static PartCommand Create(params string[] channels)
    {
        return new PartCommand
        {
            Channels = channels.ToList()
        };
    }

    /// <summary>
    ///     Creates a PART command to leave channels with an optional part message
    /// </summary>
    /// <param name="partMessage">Reason for leaving</param>
    /// <param name="channels">Channels to leave</param>
    public static PartCommand CreateWithMessage(string partMessage, params string[] channels)
    {
        return new PartCommand
        {
            Channels = channels.ToList(),
            PartMessage = partMessage
        };
    }

    /// <summary>
    ///     Creates a PART command to notify channel members about a user leaving
    /// </summary>
    /// <param name="userMask">Full user mask of the leaving user (nick!user@host)</param>
    /// <param name="channels">Channels being left</param>
    /// <param name="partMessage">Optional part message/reason</param>
    public static PartCommand CreateForChannels(
        string userMask,
        IEnumerable<string> channels,
        string partMessage = null
    )
    {
        if (string.IsNullOrEmpty(userMask))
        {
            throw new ArgumentException("User mask cannot be null or empty", nameof(userMask));
        }

        if (channels == null || !channels.Any())
        {
            throw new ArgumentException("At least one channel must be specified", nameof(channels));
        }

        return new PartCommand
        {
            Source = userMask,
            Channels = channels.ToList(),
            PartMessage = partMessage
        };
    }

    /// <summary>
    ///     Creates a PART command to notify channel members about a user leaving
    /// </summary>
    /// <param name="userMask">Full user mask of the leaving user (nick!user@host)</param>
    /// <param name="channel">Channel being left</param>
    /// <param name="partMessage">Optional part message/reason</param>
    public static PartCommand CreateForChannel(
        string userMask,
        string channel,
        string partMessage = null
    )
    {
        return CreateForChannels(userMask, new[] { channel }, partMessage);
    }
}
