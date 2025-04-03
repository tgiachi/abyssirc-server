using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_ENDOFINVITELIST (337) numeric reply indicating the end of invite list
/// </summary>
public class RplEndOfInviteList : BaseIrcCommand
{
    public RplEndOfInviteList() : base("337")
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

    public override void Parse(string line)
    {
        // Example: :server.com 337 nickname :End of /INVITE list
        var parts = line.Split(' ', 4);

        if (parts.Length < 3)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "337"
        Nickname = parts[2];
    }

    public override string Write()
    {
        return $":{ServerName} 337 {Nickname} :End of /INVITE list";
    }

    /// <summary>
    ///     Creates a RPL_ENDOFINVITELIST reply
    /// </summary>
    public static RplEndOfInviteList Create(string serverName, string nickname)
    {
        return new RplEndOfInviteList
        {
            ServerName = serverName,
            Nickname = nickname
        };
    }
}
