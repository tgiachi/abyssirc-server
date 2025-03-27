using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_ENDOFSTATS (219) numeric reply sent at the end of a STATS listing
/// </summary>
public class RplEndOfStats : BaseIrcCommand
{
    public RplEndOfStats() : base("219")
    {
    }

    /// <summary>
    ///     The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The stats query letter/character that was requested
    /// </summary>
    public char StatsQuery { get; set; }

    /// <summary>
    ///     The message to send
    /// </summary>
    public string Message { get; set; } = "End of /STATS report";

    public override void Parse(string line)
    {
        // Example: :server.com 219 nickname c :End of /STATS report
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "219"
        Nickname = parts[2];

        if (parts[3].Length > 0)
        {
            StatsQuery = parts[3][0];
        }

        Message = parts[4].TrimStart(':');
    }

    public override string Write()
    {
        return $":{ServerName} 219 {Nickname} {StatsQuery} :{Message}";
    }

    /// <summary>
    ///     Creates an RPL_ENDOFSTATS reply
    /// </summary>
    public static RplEndOfStats Create(string serverName, string nickname, char statsQuery, string message = null)
    {
        return new RplEndOfStats
        {
            ServerName = serverName,
            Nickname = nickname,
            StatsQuery = statsQuery,
            Message = message ?? "End of /STATS report"
        };
    }
}
