using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_CREATED (003) numeric reply
/// Provides information about when the server was created
/// </summary>
public class RplCreated : BaseIrcCommand
{
    /// <summary>
    /// The server name sending the reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client receiving the reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The creation message describing when the server was created
    /// </summary>
    public string CreationMessage { get; set; }

    /// <summary>
    /// The exact timestamp of server creation
    /// </summary>
    public DateTime CreationTime { get; set; }

    public RplCreated() : base("003")
    {
    }

    public override void Parse(string line)
    {
        // Reset existing data
        ServerName = null;
        Nickname = null;
        CreationMessage = null;
        CreationTime = default;

        // Ensure line starts with server prefix
        if (!line.StartsWith(':'))
        {
            return; // Invalid format
        }

        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        // Parse server name
        ServerName = parts[0].TrimStart(':');

        // Verify this is a 003 numeric
        if (parts[1] != "003")
        {
            return;
        }

        // Parse nickname
        Nickname = parts[2];

        // Parse creation message
        // Remove leading : if present
        CreationMessage = parts[3].StartsWith(':')
            ? parts[3].Substring(1)
            : parts[3];

        // Try to parse the creation timestamp if possible
        try
        {
            // Look for a date-like pattern in the message
            var dateMatch = System.Text.RegularExpressions.Regex.Match(
                CreationMessage,
                @"(\w{3}\s+\w{3}\s+\d{1,2}\s+\d{4}\s+at\s+\d{1,2}:\d{2}:\d{2})"
            );

            if (dateMatch.Success)
            {
                CreationTime = DateTime.Parse(dateMatch.Groups[1].Value);
            }
        }
        catch
        {
            // If parsing fails, keep CreationTime as default
        }
    }

    public override string Write()
    {
        // Ensure required fields are set
        if (string.IsNullOrEmpty(ServerName) ||
            string.IsNullOrEmpty(Nickname) ||
            string.IsNullOrEmpty(CreationMessage))
        {
            throw new InvalidOperationException("All fields must be set before writing");
        }

        // Construct the creation reply
        return $":{ServerName} 003 {Nickname} :{CreationMessage}";
    }

    /// <summary>
    /// Creates a RPL_CREATED reply
    /// </summary>
    /// <param name="serverName">Server sending the reply</param>
    /// <param name="nickname">Nickname receiving the reply</param>
    /// <param name="creationMessage">Message about server creation</param>
    public static RplCreated Create(
        string serverName,
        string nickname,
        string creationMessage = null)
    {
        // If no message provided, generate a default one
        creationMessage ??= $"This server was created {DateTime.Now:ddd MMM dd yyyy} at {DateTime.Now:HH:mm:ss} UTC";

        return new RplCreated
        {
            ServerName = serverName,
            Nickname = nickname,
            CreationMessage = creationMessage,
            CreationTime = DateTime.UtcNow
        };
    }
}
