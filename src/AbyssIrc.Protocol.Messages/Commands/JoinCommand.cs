using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbyssIrc.Protocol.Messages.Commands.Base;
using AbyssIrc.Protocol.Messages.Data.Channels;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC JOIN command for joining channels
/// </summary>
public class JoinCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the JOIN command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// List of channels to join with their optional keys
    /// </summary>
    public List<JoinChannelData> Channels { get; set; } = new List<JoinChannelData>();

    public JoinCommand() : base("JOIN")
    {
    }

    /// <summary>
    /// Parses a JOIN command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Channels.Clear();
        Source = null;

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

        // First token should be "JOIN"
        if (parts.Length == 0 || parts[0].ToUpper() != "JOIN")
            return;

        // Check if there are multiple channels or multiple keys
        if (parts.Length >= 2)
        {
            // Split channels
            var channels = parts[1].Split(',');

            // Check for keys if present
            if (parts.Length >= 3)
            {
                var keys = parts[2].Split(',');

                // Add channels with corresponding keys
                for (int i = 0; i < channels.Length; i++)
                {
                    string key = i < keys.Length ? keys[i] : null;
                    Channels.Add(new JoinChannelData(channels[i], key));
                }
            }
            else
            {
                // Add channels without keys
                Channels.AddRange(channels.Select(c => new JoinChannelData(c)));
            }
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted JOIN command string</returns>
    public override string Write()
    {
        var channelNames = Channels.Select(c => c.ChannelName);
        var keys = Channels.Select(c => c.Key);

        // With source (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            return HasKeys()
                ? $":{Source} JOIN {string.Join(",", channelNames)} {string.Join(",", keys)}"
                : $":{Source} JOIN {string.Join(",", channelNames)}";
        }

        // Client-side
        return HasKeys()
            ? $"JOIN {string.Join(",", channelNames)} {string.Join(",", keys)}"
            : $"JOIN {string.Join(",", channelNames)}";
    }

    /// <summary>
    /// Checks if the JOIN command has keys for channels
    /// </summary>
    private bool HasKeys()
    {
        return Channels.Any(c => !string.IsNullOrEmpty(c.Key));
    }

    /// <summary>
    /// Creates a JOIN command to join specified channels
    /// </summary>
    /// <param name="channels">Channels to join</param>
    public static JoinCommand Create(params string[] channels)
    {
        return new JoinCommand
        {
            Channels = channels.Select(c => new JoinChannelData(c)).ToList()
        };
    }

    /// <summary>
    /// Creates a JOIN command to join channels with keys
    /// </summary>
    /// <param name="channelsWithKeys">Dictionary of channels and their keys</param>
    public static JoinCommand CreateWithKeys(Dictionary<string, string> channelsWithKeys)
    {
        return new JoinCommand
        {
            Channels = channelsWithKeys.Select(
                    kvp =>
                        new JoinChannelData(kvp.Key, kvp.Value)
                )
                .ToList()
        };
    }

    /// <summary>
    /// Creates a JOIN notification for multiple channels
    /// </summary>
    /// <param name="userMask">Full user mask (nick!user@host)</param>
    /// <param name="channels">Channels being joined</param>
    public static JoinCommand CreateForChannels(string userMask, params string[] channels)
    {
        return new JoinCommand
        {
            Source = userMask,
            Channels = channels.ToList().Select(s => new JoinChannelData(s)).ToList()
        };
    }
}
