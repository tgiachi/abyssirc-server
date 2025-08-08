namespace AbyssIrc.Server.Core.Json.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Custom JSON converter that parses a string containing numbers and ranges into an integer array
/// Supports formats like: "1234,1235,1236-1239" which becomes [1234, 1235, 1236, 1237, 1238, 1239]
/// </summary>
public class NumberRangeConverter : JsonConverter<int[]>
{
    /// <summary>
    /// Reads and converts the JSON string to an integer array
    /// </summary>
    public override int[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token, got {reader.TokenType}");
        }

        string value = reader.GetString();
        return string.IsNullOrWhiteSpace(value) ? [] : ParseNumberRange(value);
    }

    /// <summary>
    /// Writes the integer array back to JSON (as a comma-separated string with ranges where applicable)
    /// </summary>
    public override void Write(Utf8JsonWriter writer, int[] value, JsonSerializerOptions options)
    {
        if (value == null || value.Length == 0)
        {
            writer.WriteStringValue(string.Empty);
            return;
        }

        // Convert back to compact string format
        string compactFormat = ConvertToCompactFormat(value);
        writer.WriteStringValue(compactFormat);
    }

    /// <summary>
    /// Parses a string containing numbers and ranges into an integer array
    /// </summary>
    public static int[] ParseNumberRange(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        var result = new List<int>();

        // Split by comma and process each part
        string[] parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            string trimmedPart = part.Trim();

            if (trimmedPart.Contains('-'))
            {
                // Handle range (e.g., "1236-1239")
                string[] rangeParts = trimmedPart.Split('-', StringSplitOptions.RemoveEmptyEntries);

                if (rangeParts.Length != 2)
                {
                    throw new FormatException($"Invalid range format: {trimmedPart}");
                }

                if (!int.TryParse(rangeParts[0].Trim(), out int start))
                {
                    throw new FormatException($"Invalid start number in range: {rangeParts[0]}");
                }

                if (!int.TryParse(rangeParts[1].Trim(), out int end))
                {
                    throw new FormatException($"Invalid end number in range: {rangeParts[1]}");
                }

                if (start > end)
                {
                    throw new FormatException($"Invalid range: start ({start}) is greater than end ({end})");
                }

                // Add all numbers in the range
                for (int i = start; i <= end; i++)
                {
                    result.Add(i);
                }
            }
            else
            {
                // Handle single number
                if (!int.TryParse(trimmedPart, out int number))
                {
                    throw new FormatException($"Invalid number: {trimmedPart}");
                }

                result.Add(number);
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// Converts an integer array back to compact string format with ranges
    /// </summary>
    private static string ConvertToCompactFormat(int[] numbers)
    {
        if (numbers.Length == 0)
            return string.Empty;

        var sorted = numbers.OrderBy(x => x).Distinct().ToList();
        var parts = new List<string>();

        int i = 0;
        while (i < sorted.Count)
        {
            int start = sorted[i];
            int end = start;

            // Find consecutive numbers
            while (i + 1 < sorted.Count && sorted[i + 1] == end + 1)
            {
                i++;
                end = sorted[i];
            }

            // Add to result
            if (start == end)
            {
                parts.Add(start.ToString());
            }
            else if (end == start + 1)
            {
                // For just two consecutive numbers, list them separately
                parts.Add(start.ToString());
                parts.Add(end.ToString());
            }
            else
            {
                // For ranges of 3 or more, use the range notation
                parts.Add($"{start}-{end}");
            }

            i++;
        }

        return string.Join(",", parts);
    }
}
