using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_LUSERCHANNELS (254) numeric reply showing channel count
/// </summary>
public class RplLuserChannels : BaseIrcCommand
{
    public RplLuserChannels() : base("254")
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
    ///     Number of channels formed
    /// </summary>
    public int ChannelCount { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 254 nickname 15 :channels formed
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "254"
        Nickname = parts[2];

        if (int.TryParse(parts[3], out var channelCount))
        {
            ChannelCount = channelCount;
        }
    }

    public override string Write()
    {
        return $":{ServerName} 254 {Nickname} {ChannelCount} :channels formed";
    }

    /// <summary>
    ///     Creates an RPL_LUSERCHANNELS reply
    /// </summary>
    public static RplLuserChannels Create(string serverName, string nickname, int channelCount)
    {
        return new RplLuserChannels
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelCount = channelCount
        };
    }
}
