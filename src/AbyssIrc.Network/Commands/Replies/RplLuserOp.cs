using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_LUSEROP (252) numeric reply showing IRC operator count
/// </summary>
public class RplLuserOp : BaseIrcCommand
{
    public RplLuserOp() : base("252")
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
    ///     Number of IRC operators online
    /// </summary>
    public int OperatorCount { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 252 nickname 1 :operator(s) online
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "252"
        Nickname = parts[2];

        if (int.TryParse(parts[3], out var opCount))
        {
            OperatorCount = opCount;
        }
    }

    public override string Write()
    {
        return $":{ServerName} 252 {Nickname} {OperatorCount} :operator(s) online";
    }

    /// <summary>
    ///     Creates an RPL_LUSEROP reply
    /// </summary>
    public static RplLuserOp Create(string serverName, string nickname, int operatorCount)
    {
        return new RplLuserOp
        {
            ServerName = serverName,
            Nickname = nickname,
            OperatorCount = operatorCount
        };
    }
}
