using System.Text.RegularExpressions;

namespace AbyssIrc.Server.Servers.Utils;

/// <summary>
/// Utility class for converting text formatting notations to mIRC caret notation
/// </summary>
public static partial class MircNotationConverter
{
    [GeneratedRegex(@"\x03(\d{1,2})(?:,(\d{1,2}))?")]
    private static partial Regex ColorRegex();

    /// <summary>
    /// Mapping of text formatting names to their caret notation equivalents
    /// </summary>
    private static readonly Dictionary<string, string> FormattingMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Control codes
            { "BOLD", "\x02" },
            { "ITALIC", "\x09" },
            { "UNDERLINE", "\x15" },
            { "STRIKE", "\x13" },
            { "REVERSE", "\x16" },
            { "RESET", "\x0f" },

            // Colors
            { "WHITE", "\x0300" },
            { "BLACK", "\x0301" },
            { "BLUE", "\x0302" },
            { "GREEN", "\x0303" },
            { "RED", "\x0304" },
            { "BROWN", "\x0305" },
            { "PURPLE", "\x0306" },
            { "ORANGE", "\x0307" },
            { "YELLOW", "\x0308" },
            { "LIGHTGREEN", "\x0309" },
            { "CYAN", "\x0310" },
            { "LIGHTCYAN", "\x0311" },
            { "LIGHTBLUE", "\x0312" },
            { "PINK", "\x0313" },
            { "GREY", "\x0314" },
            { "LIGHTGREY", "\x0315" }
        };

    /// <summary>
    /// Converts text formatting notation to mIRC caret notation
    /// </summary>
    /// <param name="input">Input text with formatting tags</param>
    /// <returns>Text with mIRC caret notation</returns>
    public static string Convert(string input)
    {
        // Replace formatting tags with caret notation
        return Regex.Replace(
            input,
            @"\[(\w+)(?:\s+(\w+))?\]",
            match =>
            {
                string tag = match.Groups[1].Value.ToUpper();

                // Check if it's a single formatting tag
                if (FormattingMap.TryGetValue(tag, out string notation))
                {
                    return notation;
                }

                // Check for color combinations
                if (match.Groups[2].Success)
                {
                    string colorTag = match.Groups[2].Value.ToUpper();

                    // Foreground,Background color notation
                    if (FormattingMap.TryGetValue(tag, out string fgColor) &&
                        FormattingMap.TryGetValue(colorTag, out string bgColor))
                    {
                        // Extract numeric color codes
                        int fgCode = int.Parse(fgColor.Substring(2));
                        int bgCode = int.Parse(bgColor.Substring(2));

                        return $"\x03{fgCode},{bgCode}";
                    }
                }

                // If no match found, return the original text
                return match.Value;
            }
        );
    }

    /// <summary>
    /// Converts mIRC caret notation back to text formatting tags
    /// </summary>
    /// <param name="input">Input text with mIRC caret notation</param>
    /// <returns>Text with formatting tags</returns>
    public static string ConvertBack(string input)
    {
        // Create a reverse mapping for conversion back
        var reverseMap = FormattingMap
            .ToDictionary(x => x.Value, x => x.Key);

        // Replace caret notation with tags
        return Regex.Replace(
            input,
            @"\x02|\x09|\x15|\x13|\x16|\x0f|\x03\d{1,2}(?:,\d{1,2})?",
            match =>
            {
                // Simple control codes
                if (reverseMap.TryGetValue(match.Value, out string tag))
                {
                    return $"[{tag}]";
                }

                // Color codes
                if (match.Value.StartsWith("\x03"))
                {
                    var colorMatch = ColorRegex().Match(match.Value);
                    if (colorMatch.Success)
                    {
                        // Get foreground color
                        string fgColor = reverseMap.FirstOrDefault(
                                x =>
                                    x.Key.EndsWith(colorMatch.Groups[1].Value)
                            )
                            .Value ?? "UNKNOWN";

                        // If background color exists
                        if (colorMatch.Groups[2].Success)
                        {
                            string bgColor = reverseMap.FirstOrDefault(
                                    x =>
                                        x.Key.EndsWith(colorMatch.Groups[2].Value)
                                )
                                .Value ?? "UNKNOWN";

                            return $"[{fgColor} {bgColor}]";
                        }

                        return $"[{fgColor}]";
                    }
                }

                return match.Value;
            }
        );
    }
}
