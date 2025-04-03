using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_ADMINLOC1 (257) numeric reply showing server location info
/// </summary>
public class RplAdminLoc1 : BaseIrcCommand
{
    public RplAdminLoc1() : base("257")
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
    ///     The server location info (city/state)
    /// </summary>
    public string LocationInfo { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 257 nickname :New York, NY, USA
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "257"
        Nickname = parts[2];
        LocationInfo = parts[3].TrimStart(':');
    }

    public override string Write()
    {
        return $":{ServerName} 257 {Nickname} :{LocationInfo}";
    }

    /// <summary>
    ///     Creates an RPL_ADMINLOC1 reply
    /// </summary>
    public static RplAdminLoc1 Create(string serverName, string nickname, string locationInfo)
    {
        return new RplAdminLoc1
        {
            ServerName = serverName,
            Nickname = nickname,
            LocationInfo = locationInfo
        };
    }
}
