using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_WHOISIDLE (317) numeric reply that shows a user's idle time
/// </summary>
public class RplWhoisIdle : BaseIrcCommand
{
    public RplWhoisIdle() : base("317")
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
    ///     The queried nickname for WHOIS
    /// </summary>
    public string QueriedNick { get; set; }

    /// <summary>
    ///     The idle time in seconds
    /// </summary>
    public int IdleSeconds { get; set; }

    /// <summary>
    ///     The Unix timestamp of when the user signed on
    /// </summary>
    public long SignOnTimestamp { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 317 nickname querieduser 12345 1621234567 :seconds idle, signon time
        var parts = line.Split(' ');

        if (parts.Length < 6)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "317"
        Nickname = parts[2];
        QueriedNick = parts[3];

        if (int.TryParse(parts[4], out var idleSeconds))
        {
            IdleSeconds = idleSeconds;
        }

        if (long.TryParse(parts[5], out var signOnTimestamp))
        {
            SignOnTimestamp = signOnTimestamp;
        }
    }

    public override string Write()
    {
        return $":{ServerName} 317 {Nickname} {QueriedNick} {IdleSeconds} {SignOnTimestamp} :seconds idle, signon time";
    }

    /// <summary>
    ///     Creates a RPL_WHOISIDLE reply
    /// </summary>
    public static RplWhoisIdle Create(
        string serverName, string nickname, string queriedNick,
        int idleSeconds, long signOnTimestamp
    )
    {
        return new RplWhoisIdle
        {
            ServerName = serverName,
            Nickname = nickname,
            QueriedNick = queriedNick,
            IdleSeconds = idleSeconds,
            SignOnTimestamp = signOnTimestamp
        };
    }

    /// <summary>
    ///     Creates a RPL_WHOISIDLE reply with a DateTime for signon time
    /// </summary>
    public static RplWhoisIdle Create(
        string serverName, string nickname, string queriedNick,
        int idleSeconds, DateTime signOnTime
    )
    {
        // Convert DateTime to Unix timestamp
        var dto = new DateTimeOffset(signOnTime.ToUniversalTime());
        var timestamp = dto.ToUnixTimeSeconds();

        return Create(serverName, nickname, queriedNick, idleSeconds, timestamp);
    }
}
