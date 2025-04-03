using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
/// Represents RPL_BANLIST (367) numeric reply
/// Lists ban masks for a channel
/// Format: ":server 367 nickname #channel banmask bansetby bantime"
/// </summary>
public class RplBanList : BaseIrcCommand
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
    /// The channel name
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// The ban mask (nick!user@host pattern)
    /// </summary>
    public string BanMask { get; set; }

    /// <summary>
    /// The nickname that set the ban
    /// </summary>
    public string BanSetBy { get; set; }

    /// <summary>
    /// The Unix timestamp when the ban was set
    /// </summary>
    public long? BanTime { get; set; }

    public RplBanList() : base("367")
    {
    }

    public override void Parse(string line)
    {
        // Example: :irc.server.net 367 nickname #channel *!*@baduser.com operator 1609459200
        var parts = line.Split(' ');

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "367"
        Nickname = parts[2];
        ChannelName = parts[3];
        BanMask = parts[4];

        // Extract ban setter if present
        if (parts.Length > 5)
        {
            BanSetBy = parts[5];
        }

        // Extract ban time if present
        if (parts.Length > 6 && long.TryParse(parts[6], out var banTime))
        {
            BanTime = banTime;
        }
    }

    public override string Write()
    {
        // Base format without ban setter and time
        var result = $":{ServerName} 367 {Nickname} {ChannelName} {BanMask}";

        // Add ban setter if present
        if (!string.IsNullOrEmpty(BanSetBy))
        {
            result += $" {BanSetBy}";

            // Add ban time if present
            if (BanTime.HasValue)
            {
                result += $" {BanTime.Value}";
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a RPL_BANLIST reply with basic information
    /// </summary>
    public static RplBanList Create(
        string serverName,
        string nickname,
        string channelName,
        string banMask)
    {
        return new RplBanList
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            BanMask = banMask
        };
    }

    /// <summary>
    /// Creates a RPL_BANLIST reply with complete information
    /// </summary>
    public static RplBanList CreateComplete(
        string serverName,
        string nickname,
        string channelName,
        string banMask,
        string banSetBy,
        long? banTime = null)
    {
        return new RplBanList
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            BanMask = banMask,
            BanSetBy = banSetBy,
            BanTime = banTime
        };
    }

    /// <summary>
    /// Creates a RPL_BANLIST reply with complete information using DateTime instead of Unix timestamp
    /// </summary>
    public static RplBanList CreateComplete(
        string serverName,
        string nickname,
        string channelName,
        string banMask,
        string banSetBy,
        DateTime? banTime)
    {
        long? timestamp = null;
        if (banTime.HasValue)
        {
            var dto = new DateTimeOffset(banTime.Value.ToUniversalTime());
            timestamp = dto.ToUnixTimeSeconds();
        }

        return CreateComplete(serverName, nickname, channelName, banMask, banSetBy, timestamp);
    }
}
