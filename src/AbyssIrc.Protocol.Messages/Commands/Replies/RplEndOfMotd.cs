using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents the RPL_ENDOFMOTD (376) numeric reply that indicates the end of MOTD
/// </summary>
public class RplEndOfMotd : BaseIrcCommand
{
    public RplEndOfMotd() : base("376")
    {
    }


    public RplEndOfMotd(string serverName, string nickname) : base("376")
    {
        ServerName = serverName;
        Nickname = nickname;
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
        // Example: :server.com 376 nickname :End of /MOTD command.
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "376"
        Nickname = parts[2];
    }

    public override string Write()
    {
        return $":{ServerName} 376 {Nickname} :End of /MOTD command.";
    }

    /// <summary>
    ///     Creates a RPL_ENDOFMOTD reply
    /// </summary>
    public static RplEndOfMotd Create(string serverName, string nickname)
    {
        return new RplEndOfMotd
        {
            ServerName = serverName,
            Nickname = nickname
        };
    }
}
