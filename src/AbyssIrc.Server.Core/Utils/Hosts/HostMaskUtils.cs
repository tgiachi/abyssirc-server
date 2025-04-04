using System.Text.RegularExpressions;

namespace AbyssIrc.Server.Core.Utils.Hosts;

/// <summary>
/// Utility class for working with IRC host masks (nick!user@host patterns)
/// </summary>
public static class HostMaskUtils
{
    /// <summary>
    /// Checks if a host mask pattern matches a given user mask
    /// </summary>
    /// <param name="hostMaskPattern">The host mask pattern to check (e.g. "user@*.tim.it" or "*!*@example.com")</param>
    /// <param name="userMask">The full user mask to match against (format: "nick!user@host")</param>
    /// <returns>True if the pattern matches the user mask, false otherwise</returns>
    public static bool IsHostMaskMatch(string hostMaskPattern, string userMask)
    {
        // Empty pattern is invalid
        if (string.IsNullOrWhiteSpace(hostMaskPattern))
            return false;

        // "*" or "*@*" or "*!*@*" matches everything
        if (hostMaskPattern == "*" || hostMaskPattern == "*@*" || hostMaskPattern == "*!*@*")
            return true;

        // Split the user mask into nick, user, host components
        string userNick = string.Empty;
        string userUser = string.Empty;
        string userHost = string.Empty;

        // Extract components from user mask
        ParseUserMask(userMask, out userNick, out userUser, out userHost);

        // Parse the hostMaskPattern into its components
        string patternNick = string.Empty;
        string patternUser = string.Empty;
        string patternHost = string.Empty;

        // Check for nick!user@host format
        int exclamationIndex = hostMaskPattern.IndexOf('!');
        int atIndex = hostMaskPattern.IndexOf('@');

        // Format: nick!user@host
        if (exclamationIndex >= 0 && atIndex > exclamationIndex)
        {
            patternNick = hostMaskPattern.Substring(0, exclamationIndex);
            patternUser = hostMaskPattern.Substring(exclamationIndex + 1, atIndex - exclamationIndex - 1);
            patternHost = hostMaskPattern.Substring(atIndex + 1);

            return MatchPattern(patternNick, userNick) &&
                   MatchPattern(patternUser, userUser) &&
                   MatchPattern(patternHost, userHost);
        }
        // Format: nick!user
        else if (exclamationIndex >= 0)
        {
            patternNick = hostMaskPattern.Substring(0, exclamationIndex);
            patternUser = hostMaskPattern.Substring(exclamationIndex + 1);

            return MatchPattern(patternNick, userNick) &&
                   MatchPattern(patternUser, userUser);
        }
        // Format: user@host
        else if (atIndex >= 0)
        {
            patternUser = hostMaskPattern.Substring(0, atIndex);
            patternHost = hostMaskPattern.Substring(atIndex + 1);

            // Important: In this case, we explicitly check against user and host parts
            // regardless of what nickname might be present
            return MatchPattern(patternUser, userUser) &&
                   MatchPattern(patternHost, userHost);
        }
        // Format: just host or user
        else
        {
            // Without @ or !, treat as host pattern
            patternHost = hostMaskPattern;
            return MatchPattern(patternHost, userHost);
        }
    }

    /// <summary>
    /// Parses a user mask string into its components
    /// </summary>
    /// <param name="userMask">The user mask to parse (format: "nick!user@host")</param>
    /// <param name="nick">Output parameter for the nickname</param>
    /// <param name="user">Output parameter for the username</param>
    /// <param name="host">Output parameter for the hostname</param>
    public static void ParseUserMask(string userMask, out string nick, out string user, out string host)
    {
        nick = string.Empty;
        user = string.Empty;
        host = string.Empty;

        // Invalid mask
        if (string.IsNullOrWhiteSpace(userMask))
            return;

        var exclamationIndex = userMask.IndexOf('!');
        var atIndex = userMask.IndexOf('@');

        // Simple host with no nick or user parts
        if (exclamationIndex < 0 && atIndex < 0)
        {
            host = userMask;
            return;
        }

        // Format: user@host
        if (exclamationIndex < 0 && atIndex >= 0)
        {
            user = userMask.Substring(0, atIndex);
            host = userMask.Substring(atIndex + 1);
            return;
        }

        // Format: nick!user or nick!user@host
        if (exclamationIndex >= 0)
        {
            nick = userMask.Substring(0, exclamationIndex);

            if (atIndex > exclamationIndex)
            {
                user = userMask.Substring(exclamationIndex + 1, atIndex - exclamationIndex - 1);
                host = userMask.Substring(atIndex + 1);
            }
            else
            {
                user = userMask.Substring(exclamationIndex + 1);
            }
        }
    }

    /// <summary>
    /// Matches a pattern against a value using IRC-style wildcards
    /// </summary>
    /// <param name="pattern">The pattern which may include * and ? wildcards</param>
    /// <param name="value">The value to check against the pattern</param>
    /// <returns>True if the pattern matches the value</returns>
    private static bool MatchPattern(string pattern, string value)
    {
        // Empty pattern only matches empty value
        if (string.IsNullOrEmpty(pattern))
            return string.IsNullOrEmpty(value);

        // "*" matches anything
        if (pattern == "*")
            return true;

        // Convert IRC-style pattern to regex
        // * => .*
        // ? => .
        // Escape special regex characters
        string regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase);
    }
}
