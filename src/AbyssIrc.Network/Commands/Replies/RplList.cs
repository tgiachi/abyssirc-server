using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_LIST (322) numeric reply that shows information about a channel
/// </summary>
public class RplList : BaseIrcCommand
{
    public RplList() : base("322")
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
    ///     The number of visible users in the channel
    /// </summary>
    public int VisibleCount { get; set; }

    /// <summary>
    ///     The channel topic
    /// </summary>
    public string Topic { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 322 nickname #channel 42 :Channel topic goes here
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "322"
        Nickname = parts[2];
        ChannelName = parts[3];

        if (int.TryParse(parts[4].Split(' ')[0], out var visibleCount))
        {
            VisibleCount = visibleCount;
        }

        // Extract topic from the remainder
        var topicStart = line.IndexOf(':', parts[0].Length + 1);
        if (topicStart != -1)
        {
            Topic = line.Substring(topicStart + 1);
        }
    }

    public override string Write()
    {
        return $":{ServerName} 322 {Nickname} {ChannelName} {VisibleCount} :{Topic}";
    }

    /// <summary>
    ///     Creates a RPL_LIST reply
    /// </summary>
    public static RplList Create(string serverName, string nickname, string channelName, int visibleCount, string topic)
    {
        return new RplList
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            VisibleCount = visibleCount,
            Topic = topic ?? string.Empty
        };
    }
}
