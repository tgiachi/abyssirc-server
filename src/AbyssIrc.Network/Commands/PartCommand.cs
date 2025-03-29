using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents an IRC PART command for leaving channels
/// </summary>
public class PartCommand : BaseIrcCommand
{
    /// <summary>
    /// Source of the PART command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// List of channels to leave
    /// </summary>
    public List<string> Channels { get; set; } = new List<string>();

    /// <summary>
    /// Part message (optional reason for leaving)
    /// </summary>
    public string PartMessage { get; set; }

    public PartCommand() : base("PART")
    {
    }

    /// <summary>
    /// Parses a PART command from a raw IRC message
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
            int spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                Source = line.Substring(1, spaceIndex - 1);
                line = line.Substring(spaceIndex + 1).TrimStart();
            }
        }

        // Split remaining parts
        string[] parts = line.Split(' ');

        // First token should be "PART"
        if (parts.Length == 0 || parts[0].ToUpper() != "PART")
            return;

        // Check for channels
        if (parts.Length >= 2)
        {
            // Split channels
            var channels = parts[1].Split(',');
            Channels.AddRange(channels);

            // Check for part message
            int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + 2);
            if (colonIndex != -1)
            {
                PartMessage = line.Substring(colonIndex + 1);
            }
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted PART command string</returns>
    public override string Write()
    {
        // Prepare base command
        StringBuilder commandBuilder = new StringBuilder();

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
    /// Creates a PART command to leave specified channels
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
    /// Creates a PART command to leave channels with an optional part message
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
}
