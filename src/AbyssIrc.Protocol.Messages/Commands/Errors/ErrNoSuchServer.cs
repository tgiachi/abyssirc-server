using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents ERR_NOSUCHSERVER (402) numeric reply
/// </summary>
public class ErrNoSuchServer : BaseIrcCommand
{
    public ErrNoSuchServer() : base("402")
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
    ///     The server name that does not exist
    /// </summary>
    public string TargetServer { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 402 nickname target.server :No such server
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "402"
        Nickname = parts[2];
        TargetServer = parts[3].Split(' ')[0];
    }

    public override string Write()
    {
        return $":{ServerName} {Code} {Nickname} {TargetServer} :No such server";
    }

    /// <summary>
    ///     Creates an ERR_NOSUCHSERVER reply
    /// </summary>
    public static ErrNoSuchServer Create(string serverName, string nickname, string targetServer)
    {
        return new ErrNoSuchServer
        {
            ServerName = serverName,
            Nickname = nickname,
            TargetServer = targetServer
        };
    }
}
