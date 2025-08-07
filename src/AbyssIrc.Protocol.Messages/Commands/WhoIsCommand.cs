using System.Text;
using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents an IRC WHOIS command used to query information about a specific user
///     Format: "WHOIS [target] nickname" or "WHOIS nickname"
/// </summary>
public class WhoIsCommand : BaseIrcCommand
{
    public WhoIsCommand() : base("WHOIS")
    {
    }

    /// <summary>
    ///     The source of the WHOIS command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The target server to direct the WHOIS query to (optional)
    /// </summary>
    public string TargetServer { get; set; }

    /// <summary>
    ///     The nickname(s) being queried
    /// </summary>
    public List<string> Nicknames { get; set; } = new();

    public override void Parse(string line)
    {
        // Reset existing data
        Source = null;
        TargetServer = null;
        Nicknames.Clear();

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

        // First token should be "WHOIS"
        if (parts.Length < 2 || parts[0].ToUpper() != "WHOIS")
        {
            return;
        }

        // Different formats:
        // WHOIS nickname
        // WHOIS target nickname
        // WHOIS nickname1,nickname2,nickname3

        if (parts.Length >= 3)
        {
            // Check if the first parameter might be a target server
            // It's ambiguous, but the convention is that if there are two parameters,
            // and the second doesn't contain a comma, the first is a target server
            if (!parts[2].Contains(','))
            {
                TargetServer = parts[1];
                // The rest are nicknames
                Nicknames.AddRange(parts[2].Split(','));
            }
            else
            {
                // First parameter is definitely a nickname or comma-separated list
                Nicknames.AddRange(parts[1].Split(','));
                // Additional nicknames
                for (var i = 2; i < parts.Length; i++)
                {
                    Nicknames.AddRange(parts[i].Split(','));
                }
            }
        }
        else
        {
            // Only one parameter: it's a nickname or comma-separated list
            Nicknames.AddRange(parts[1].Split(','));
        }
    }

    public override string Write()
    {
        var commandBuilder = new StringBuilder();

        // Add source if provided (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add the WHOIS command
        commandBuilder.Append("WHOIS");

        // Add target server if provided
        if (!string.IsNullOrEmpty(TargetServer))
        {
            commandBuilder.Append(' ').Append(TargetServer);
        }

        // Add nicknames
        if (Nicknames.Count > 0)
        {
            commandBuilder.Append(' ').Append(string.Join(",", Nicknames));
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    ///     Creates a WHOIS command to query a single nickname
    /// </summary>
    /// <param name="nickname">The nickname to query</param>
    /// <returns>A WHOIS command for the specified nickname</returns>
    public static WhoIsCommand Create(string nickname)
    {
        return new WhoIsCommand
        {
            Nicknames = new List<string> { nickname }
        };
    }

    /// <summary>
    ///     Creates a WHOIS command with a target server
    /// </summary>
    /// <param name="targetServer">The server to query</param>
    /// <param name="nickname">The nickname to query</param>
    /// <returns>A WHOIS command targeting a specific server</returns>
    public static WhoIsCommand CreateWithTarget(string targetServer, string nickname)
    {
        return new WhoIsCommand
        {
            TargetServer = targetServer,
            Nicknames = new List<string> { nickname }
        };
    }

    /// <summary>
    ///     Creates a WHOIS command for multiple nicknames
    /// </summary>
    /// <param name="nicknames">The nicknames to query</param>
    /// <returns>A WHOIS command for the specified nicknames</returns>
    public static WhoIsCommand CreateForMultiple(IEnumerable<string> nicknames)
    {
        return new WhoIsCommand
        {
            Nicknames = new List<string>(nicknames)
        };
    }
}
