using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents an IRC OPER command used to authenticate as an IRC operator
///     Format: "OPER username password"
/// </summary>
public class OperCommand : BaseIrcCommand
{
    public OperCommand() : base("OPER")
    {
    }

    /// <summary>
    ///     The operator username
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    ///     The operator password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    ///     Source of the command (for server responses)
    /// </summary>
    public string Source { get; set; }

    public override void Parse(string line)
    {
        // Handle source prefix if present
        var parseLine = line;
        if (line.StartsWith(':'))
        {
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex == -1)
            {
                return; // Invalid format
            }

            Source = line.Substring(1, spaceIndex - 1);
            parseLine = line.Substring(spaceIndex + 1).TrimStart();
        }

        // Split into parts
        var parts = parseLine.Split(' ');

        // First token should be "OPER"
        if (parts.Length == 0 || parts[0].ToUpper() != "OPER")
        {
            return;
        }

        // Extract username and password if present
        if (parts.Length > 1)
        {
            Username = parts[1];
        }

        if (parts.Length > 2)
        {
            Password = parts[2];
        }
    }

    public override string Write()
    {
        if (!string.IsNullOrEmpty(Source))
        {
            return $":{Source} OPER {Username} {Password}";
        }

        return $"OPER {Username} {Password}";
    }

    /// <summary>
    ///     Creates an OPER command for authenticating as an operator
    /// </summary>
    /// <param name="username">Operator username</param>
    /// <param name="password">Operator password</param>
    /// <returns>A properly formatted OPER command</returns>
    public static OperCommand Create(string username, string password)
    {
        return new OperCommand
        {
            Username = username,
            Password = password
        };
    }

    /// <summary>
    ///     Creates an OPER command with source (typically for server responses)
    /// </summary>
    /// <param name="source">Command source</param>
    /// <param name="username">Operator username</param>
    /// <param name="password">Operator password</param>
    /// <returns>A properly formatted OPER command with source</returns>
    public static OperCommand CreateWithSource(string source, string username, string password)
    {
        return new OperCommand
        {
            Source = source,
            Username = username,
            Password = password
        };
    }
}
