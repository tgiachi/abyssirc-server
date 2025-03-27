using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_TOPICWHOTIME (333) numeric reply showing who set the topic and when
/// </summary>
public class RplTopicWhoTime : BaseIrcCommand
{
    public RplTopicWhoTime() : base("333")
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
    ///     The channel name
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The nick!user@host who set the topic
    /// </summary>
    public string SetterMask { get; set; }

    /// <summary>
    ///     The Unix timestamp when the topic was set
    /// </summary>
    public long SetTimestamp { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 333 nickname #channel nick!user@host 1609459200
        var parts = line.Split(' ');

        if (parts.Length < 6)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "333"
        Nickname = parts[2];
        ChannelName = parts[3];
        SetterMask = parts[4];

        if (long.TryParse(parts[5], out var timestamp))
        {
            SetTimestamp = timestamp;
        }
    }

    public override string Write()
    {
        return $":{ServerName} 333 {Nickname} {ChannelName} {SetterMask} {SetTimestamp}";
    }

    /// <summary>
    ///     Creates a RPL_TOPICWHOTIME reply
    /// </summary>
    public static RplTopicWhoTime Create(
        string serverName, string nickname, string channelName,
        string setterMask, long setTimestamp
    )
    {
        return new RplTopicWhoTime
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            SetterMask = setterMask,
            SetTimestamp = setTimestamp
        };
    }

    /// <summary>
    ///     Creates a RPL_TOPICWHOTIME reply with a DateTime
    /// </summary>
    public static RplTopicWhoTime Create(
        string serverName, string nickname, string channelName,
        string setterMask, DateTime setTime
    )
    {
        // Convert DateTime to Unix timestamp
        var dto = new DateTimeOffset(setTime.ToUniversalTime());
        var timestamp = dto.ToUnixTimeSeconds();

        return Create(serverName, nickname, channelName, setterMask, timestamp);
    }
}
