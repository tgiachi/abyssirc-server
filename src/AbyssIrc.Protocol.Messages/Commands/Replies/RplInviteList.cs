using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_INVITELIST (336) numeric reply showing channels the client is invited to
/// </summary>
public class RplInviteList : BaseIrcCommand
{
    public RplInviteList() : base("336")
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
    ///     The channel name the client is invited to
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The nickname of user who sent the invitation
    /// </summary>
    public string InviterNick { get; set; }

    /// <summary>
    ///     The Unix timestamp when the invite was issued
    /// </summary>
    public long InviteTimestamp { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 336 nickname #channel inviter 1609459200
        var parts = line.Split(' ');

        if (parts.Length < 6)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "336"
        Nickname = parts[2];
        ChannelName = parts[3];
        InviterNick = parts[4];

        if (long.TryParse(parts[5], out var timestamp))
        {
            InviteTimestamp = timestamp;
        }
    }

    public override string Write()
    {
        return $":{ServerName} 336 {Nickname} {ChannelName} {InviterNick} {InviteTimestamp}";
    }

    /// <summary>
    ///     Creates a RPL_INVITELIST reply
    /// </summary>
    public static RplInviteList Create(
        string serverName, string nickname, string channelName,
        string inviterNick, long inviteTimestamp
    )
    {
        return new RplInviteList
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            InviterNick = inviterNick,
            InviteTimestamp = inviteTimestamp
        };
    }

    /// <summary>
    ///     Creates a RPL_INVITELIST reply with a DateTime
    /// </summary>
    public static RplInviteList Create(
        string serverName, string nickname, string channelName,
        string inviterNick, DateTime inviteTime
    )
    {
        // Convert DateTime to Unix timestamp
        var dto = new DateTimeOffset(inviteTime.ToUniversalTime());
        var timestamp = dto.ToUnixTimeSeconds();

        return Create(serverName, nickname, channelName, inviterNick, timestamp);
    }
}
