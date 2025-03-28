using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

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
    /// List of channels to join
    /// </summary>
    public List<string> Channels { get; set; } = new List<string>();

    /// <summary>
    /// List of keys for password-protected channels (optional)
    /// </summary>
    public List<string> Keys { get; set; } = new List<string>();

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
        Keys.Clear();
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
            Channels.AddRange(channels);

            // Check for keys if present
            if (parts.Length >= 3)
            {
                var keys = parts[2].Split(',');

                // Add keys, padding with null if fewer keys than channels
                for (int i = 0; i < channels.Length; i++)
                {
                    Keys.Add(i < keys.Length ? keys[i] : null);
                }
            }
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted JOIN command string</returns>
    public override string Write()
    {
        // With source (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            return HasKeys()
                ? $":{Source} JOIN {string.Join(",", Channels)} {string.Join(",", Keys)}"
                : $":{Source} JOIN {string.Join(",", Channels)}";
        }

        // Client-side
        return HasKeys()
            ? $"JOIN {string.Join(",", Channels)} {string.Join(",", Keys)}"
            : $"JOIN {string.Join(",", Channels)}";
    }

    /// <summary>
    /// Checks if the JOIN command has keys for channels
    /// </summary>
    private bool HasKeys()
    {
        return Keys != null && Keys.Any(k => !string.IsNullOrEmpty(k));
    }

    /// <summary>
    /// Creates a JOIN command to join specified channels
    /// </summary>
    /// <param name="channels">Channels to join</param>
    public static JoinCommand Create(params string[] channels)
    {
        return new JoinCommand
        {
            Channels = channels.ToList()
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
            Channels = channelsWithKeys.Keys.ToList(),
            Keys = channelsWithKeys.Values.ToList()
        };
    }
}
