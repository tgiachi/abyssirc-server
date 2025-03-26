using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
/// Represents ERR_NOSUCHNICK (401) numeric reply
/// </summary>
public class ErrNoSuchNick : BaseIrcCommand
{
    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname that does not exist
    /// </summary>
    public string TargetNick { get; set; }

    public ErrNoSuchNick() : base("401")
    {
    }

    public ErrNoSuchNick(string serverName, string nickname, string targetNick) : base("401")
    {
        Nickname = nickname;
        ServerName = serverName;
        TargetNick = targetNick;
    }

    public override void Parse(string line)
    {
        // Example: :server.com 401 nickname target :No such nick/channel
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "401"
        Nickname = parts[2];
        TargetNick = parts[3].Split(' ')[0];
    }

    public override string Write()
    {
        return $":{ServerName} {Code} {Nickname} {TargetNick} :No such nick/channel";
    }

    /// <summary>
    /// Creates an ERR_NOSUCHNICK reply
    /// </summary>
    public static ErrNoSuchNick Create(string serverName, string nickname, string targetNick)
    {
        return new ErrNoSuchNick
        {
            ServerName = serverName,
            Nickname = nickname,
            TargetNick = targetNick
        };
    }
}
