using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AbyssIrc.Protocol.Messages.Commands.Base;
using AbyssIrc.Protocol.Messages.Types;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC LIST command for querying channel information
/// </summary>
public class ListCommand : BaseIrcCommand
{
    /// <summary>
    /// List of specific channels to query
    /// </summary>
    public List<string> Channels { get; set; } = new List<string>();

    /// <summary>
    /// Filter type for the LIST command
    /// </summary>
    public ListFilterType? FilterType { get; set; }

    /// <summary>
    /// Comparison type for the filter
    /// </summary>
    public ComparisonType Comparison { get; set; } = ComparisonType.GreaterThan;

    /// <summary>
    /// Numeric value for the filter
    /// </summary>
    public int? FilterValue { get; set; }

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
        Channels.Clear();
        FilterType = null;
        Comparison = ComparisonType.GreaterThan;
        FilterValue = null;

        // Split the line into parts
        var parts = line.Split(' ');

        // First token should be "LIST"
        if (parts.Length == 0 || parts[0].ToUpper() != "LIST")
            return;

        // If only LIST is provided, it means list all channels
        if (parts.Length == 1)
            return;

        // Process the first parameter
        var firstParam = parts[1];

        // Check if it's a list of channels
        if (firstParam.Contains(','))
        {
            Channels.AddRange(firstParam.Split(','));
            return;
        }

        // Check for filter patterns
        var match = Regex.Match(firstParam, @"^([CUT])?([<>=])?(\d+)$");
        if (match.Success)
        {
            // Determine filter type
            switch (match.Groups[1].Value)
            {
                case "C":
                    FilterType = ListFilterType.Created;
                    break;
                case "T":
                    FilterType = ListFilterType.TopicChanged;
                    break;
                default:
                    FilterType = ListFilterType.Users;
                    break;
            }

            // Determine comparison type
            switch (match.Groups[2].Value)
            {
                case "<":
                    Comparison = ComparisonType.LessThan;
                    break;
                case "=":
                    Comparison = ComparisonType.EqualTo;
                    break;
                default:
                    Comparison = ComparisonType.GreaterThan;
                    break;
            }

            // Parse filter value
            FilterValue = int.Parse(match.Groups[3].Value);
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted LIST command string</returns>
    public override string Write()
    {
        // If specific channels are provided
        if (Channels.Any())
        {
            return $"LIST {string.Join(",", Channels)}";
        }

        // If a filter is set
        if (FilterType.HasValue)
        {
            string typePrefix = FilterType switch
            {
                ListFilterType.Created      => "C",
                ListFilterType.TopicChanged => "T",
                _                           => ""
            };

            string comparisonSymbol = Comparison switch
            {
                ComparisonType.LessThan => "<",
                ComparisonType.EqualTo  => "=",
                _                       => ">"
            };

            return $"LIST {typePrefix}{comparisonSymbol}{FilterValue}";
        }

        // Basic LIST command
        return "LIST";
    }

    /// <summary>
    /// Creates a LIST command to list specific channels
    /// </summary>
    /// <param name="channels">Channels to list</param>
    public static ListCommand CreateForChannels(params string[] channels)
    {
        return new ListCommand
        {
            Channels = channels.ToList()
        };
    }

    /// <summary>
    /// Creates a LIST command with a user count filter
    /// </summary>
    /// <param name="count">Number of users to filter by</param>
    /// <param name="comparison">Comparison type (default is greater than)</param>
    public static ListCommand CreateByUserCount(
        int count,
        ComparisonType comparison = ComparisonType.GreaterThan
    )
    {
        return new ListCommand
        {
            FilterType = ListFilterType.Users,
            FilterValue = count,
            Comparison = comparison
        };
    }

    /// <summary>
    /// Creates a LIST command with a channel creation time filter
    /// </summary>
    /// <param name="minutes">Minutes to filter by</param>
    /// <param name="comparison">Comparison type (default is greater than)</param>
    public static ListCommand CreateByCreationTime(
        int minutes,
        ComparisonType comparison = ComparisonType.GreaterThan
    )
    {
        return new ListCommand
        {
            FilterType = ListFilterType.Created,
            FilterValue = minutes,
            Comparison = comparison
        };
    }

    /// <summary>
    /// Creates a LIST command with a topic change time filter
    /// </summary>
    /// <param name="minutes">Minutes to filter by</param>
    /// <param name="comparison">Comparison type (default is less than)</param>
    public static ListCommand CreateByTopicChangeTime(
        int minutes,
        ComparisonType comparison = ComparisonType.LessThan
    )
    {
        return new ListCommand
        {
            FilterType = ListFilterType.TopicChanged,
            FilterValue = minutes,
            Comparison = comparison
        };
    }
}
