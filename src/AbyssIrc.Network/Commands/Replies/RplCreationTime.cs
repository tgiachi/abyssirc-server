using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_CREATIONTIME (329) numeric reply that shows when a channel was created
/// </summary>
public class RplCreationTime : BaseIrcCommand
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
    /// The channel name
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// The Unix timestamp of when the channel was created
    /// </summary>
    public long CreationTimestamp { get; set; }

    public RplCreationTime() : base("329")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.com 329 nickname #channel 1609459200
        var parts = line.Split(' ');

        if (parts.Length < 5)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "329"
        Nickname = parts[2];
        ChannelName = parts[3];

        if (long.TryParse(parts[4], out long timestamp))
        {
            CreationTimestamp = timestamp;
        }
    }

    public override string Write()
    {
        return $":{ServerName} 329 {Nickname} {ChannelName} {CreationTimestamp}";
    }

    /// <summary>
    /// Creates a RPL_CREATIONTIME reply
    /// </summary>
    public static RplCreationTime Create(string serverName, string nickname, string channelName, long creationTimestamp)
    {
        return new RplCreationTime
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            CreationTimestamp = creationTimestamp
        };
    }

    /// <summary>
    /// Creates a RPL_CREATIONTIME reply with a DateTime
    /// </summary>
    public static RplCreationTime Create(string serverName, string nickname, string channelName, DateTime creationTime)
    {
        // Convert DateTime to Unix timestamp
        DateTimeOffset dto = new DateTimeOffset(creationTime.ToUniversalTime());
        long timestamp = dto.ToUnixTimeSeconds();

        return Create(serverName, nickname, channelName, timestamp);
    }
}

