using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents the RPL_TIME (391) numeric reply with server time
/// </summary>
public class RplTime : BaseIrcCommand
{
    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The server from which the time is being reported
    /// </summary>
    public string TimeServer { get; set; }

    /// <summary>
    /// The current time on the server
    /// </summary>
    public DateTime ServerTime { get; set; }

    /// <summary>
    /// The formatted time string
    /// </summary>
    public string TimeString { get; set; }

    public RplTime() : base("391")
    {
    }

    /// <summary>
    /// Parses the RPL_TIME numeric reply
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 391 nickname timeserver :Current local time is Mon Jan 01 12:34:56 2024

        // Reset existing data
        ServerName = null;
        Nickname = null;
        TimeServer = null;
        ServerTime = DateTime.MinValue;
        TimeString = null;

        // Check for source prefix
        if (line.StartsWith(':'))
        {
            int spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                ServerName = line.Substring(1, spaceIndex - 1);
                line = line.Substring(spaceIndex + 1).TrimStart();
            }
        }

        // Split remaining parts
        string[] parts = line.Split(' ');

        // Ensure we have enough parts
        if (parts.Length < 4)
        {
            return;
        }

        // Verify the numeric code
        if (parts[0] != "391")
        {
            return;
        }

        // Extract nickname
        Nickname = parts[1];

        // Extract time server
        TimeServer = parts[2];

        // Extract time string (everything after the colon)
        int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + parts[2].Length + 3);
        if (colonIndex != -1)
        {
            TimeString = line.Substring(colonIndex + 1).Trim();

            // Try to parse the time string
            if (DateTime.TryParse(TimeString, out DateTime parsedTime))
            {
                ServerTime = parsedTime;
            }
        }
    }

    /// <summary>
    /// Converts the reply to its string representation
    /// </summary>
    /// <returns>Formatted RPL_TIME message</returns>
    public override string Write()
    {
        // If no time string is set, generate one
        if (string.IsNullOrEmpty(TimeString))
        {
            ServerTime = DateTime.Now;
            TimeString = ServerTime.ToString("ddd MMM dd HH:mm:ss yyyy");
        }

        return string.IsNullOrEmpty(ServerName)
            ? $"391 {Nickname} {TimeServer} :{TimeString}"
            : $":{ServerName} 391 {Nickname} {TimeServer} :{TimeString}";
    }

    /// <summary>
    /// Creates a RPL_TIME reply
    /// </summary>
    /// <param name="serverName">Server sending the reply</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="timeServer">Server from which time is reported</param>
    /// <param name="serverTime">Optional specific time (defaults to current time)</param>
    public static RplTime Create(
        string serverName,
        string nickname,
        string timeServer,
        DateTime? serverTime = null
    )
    {
        return new RplTime
        {
            ServerName = serverName,
            Nickname = nickname,
            TimeServer = timeServer,
            ServerTime = serverTime ?? DateTime.Now,
            TimeString = (serverTime ?? DateTime.Now).ToString("ddd MMM dd HH:mm:ss yyyy")
        };
    }
}
