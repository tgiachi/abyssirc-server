using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_TOPIC (332) numeric reply showing a channel's topic
/// </summary>
public class RplTopic : BaseIrcCommand
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
    /// The channel topic
    /// </summary>
    public string Topic { get; set; }

    public RplTopic() : base("332")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.com 332 nickname #channel :This is the channel topic
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "332"
        Nickname = parts[2];
        ChannelName = parts[3].Split(' ')[0];

        // Extract topic from the remainder
        int colonPos = line.IndexOf(':', parts[0].Length);
        if (colonPos != -1)
        {
            Topic = line.Substring(colonPos + 1);
        }
    }

    public override string Write()
    {
        return $":{ServerName} 332 {Nickname} {ChannelName} :{Topic}";
    }

    /// <summary>
    /// Creates a RPL_TOPIC reply
    /// </summary>
    public static RplTopic Create(string serverName, string nickname, string channelName, string topic)
    {
        return new RplTopic
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            Topic = topic
        };
    }
}
