using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_INVITING (341) numeric reply indicating that a user has been invited to a channel
/// </summary>
public class RplInviting : BaseIrcCommand
{
    public RplInviting() : base("341")
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
    ///     The channel the user is being invited to
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The nickname of the user being invited
    /// </summary>
    public string InvitedNick { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 341 nickname #channel invitednick
        var parts = line.Split(' ');

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "341"
        Nickname = parts[2];
        ChannelName = parts[3];
        InvitedNick = parts[4];
    }

    public override string Write()
    {
        return $":{ServerName} 341 {Nickname} {ChannelName} {InvitedNick}";
    }

    /// <summary>
    ///     Creates a RPL_INVITING reply
    /// </summary>
    public static RplInviting Create(string serverName, string nickname, string channelName, string invitedNick)
    {
        return new RplInviting
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            InvitedNick = invitedNick
        };
    }
}
