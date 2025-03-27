using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_ADMINLOC2 (258) numeric reply showing server affiliation
/// </summary>
public class RplAdminLoc2 : BaseIrcCommand
{
    public RplAdminLoc2() : base("258")
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
    ///     The server affiliation or hosting info
    /// </summary>
    public string AffiliationInfo { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 258 nickname :Example Network Operations Center
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "258"
        Nickname = parts[2];
        AffiliationInfo = parts[3].TrimStart(':');
    }

    public override string Write()
    {
        return $":{ServerName} 258 {Nickname} :{AffiliationInfo}";
    }

    /// <summary>
    ///     Creates an RPL_ADMINLOC2 reply
    /// </summary>
    public static RplAdminLoc2 Create(string serverName, string nickname, string affiliationInfo)
    {
        return new RplAdminLoc2
        {
            ServerName = serverName,
            Nickname = nickname,
            AffiliationInfo = affiliationInfo
        };
    }
}
