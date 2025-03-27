using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_STATSUPTIME (242) numeric reply that shows server uptime statistics
/// </summary>
public class RplStatsUptime : BaseIrcCommand
{
    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The uptime message including days, hours, etc.
    /// </summary>
    public string UptimeMessage { get; set; }

    public RplStatsUptime() : base("242")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.com 242 nickname :Server Up 3 days, 2:34:56
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "242"
        Nickname = parts[2];
        UptimeMessage = parts[3].TrimStart(':');
    }

    public override string Write()
    {
        return $":{ServerName} 242 {Nickname} :{UptimeMessage}";
    }

    /// <summary>
    /// Creates a RPL_STATSUPTIME reply with a formatted uptime message
    /// </summary>
    public static RplStatsUptime Create(string serverName, string nickname, TimeSpan uptime)
    {
        // Format the uptime in a human-readable format
        string uptimeMessage = FormatUptime(uptime);

        return new RplStatsUptime
        {
            ServerName = serverName,
            Nickname = nickname,
            UptimeMessage = uptimeMessage
        };
    }

    /// <summary>
    /// Formats a TimeSpan into a human-readable uptime string
    /// </summary>
    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
        {
            return $"Server Up {(int)uptime.TotalDays} days, {uptime.Hours}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }
        else
        {
            return $"Server Up {uptime.Hours}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }
    }
}
