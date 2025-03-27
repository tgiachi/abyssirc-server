using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_LUSERUNKNOWN (253) numeric reply showing unknown connection count
/// </summary>
public class RplLuserUnknown : BaseIrcCommand
{
    public RplLuserUnknown() : base("253")
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
    ///     Number of unknown connections
    /// </summary>
    public int UnknownCount { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 253 nickname 1 :unknown connection(s)
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "253"
        Nickname = parts[2];

        if (int.TryParse(parts[3], out var unknownCount))
        {
            UnknownCount = unknownCount;
        }
    }

    public override string Write()
    {
        return $":{ServerName} 253 {Nickname} {UnknownCount} :unknown connection(s)";
    }

    /// <summary>
    ///     Creates an RPL_LUSERUNKNOWN reply
    /// </summary>
    public static RplLuserUnknown Create(string serverName, string nickname, int unknownCount)
    {
        return new RplLuserUnknown
        {
            ServerName = serverName,
            Nickname = nickname,
            UnknownCount = unknownCount
        };
    }
}
