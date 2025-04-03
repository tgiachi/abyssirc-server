using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_NOTOPIC (331) numeric reply indicating a channel has no topic set
/// </summary>
public class RplNoTopic : BaseIrcCommand
{
    public RplNoTopic() : base("331")
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

    public override void Parse(string line)
    {
        // Example: :server.com 331 nickname #channel :No topic is set
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "331"
        Nickname = parts[2];
        ChannelName = parts[3].Split(' ')[0];
    }

    public override string Write()
    {
        return $":{ServerName} 331 {Nickname} {ChannelName} :No topic is set";
    }

    /// <summary>
    ///     Creates a RPL_NOTOPIC reply
    /// </summary>
    public static RplNoTopic Create(string serverName, string nickname, string channelName)
    {
        return new RplNoTopic
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName
        };
    }
}
