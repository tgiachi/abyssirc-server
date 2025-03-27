using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
///     Represents ERR_NICKNAMEINUSE (433) numeric reply
/// </summary>
public class ErrNicknameInUse : BaseIrcCommand
{
    public ErrNicknameInUse() : base("433")
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
    ///     The nickname that is already in use
    /// </summary>
    public string RequestedNick { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 433 * requested_nick :Nickname is already in use
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "433"
        Nickname = parts[2]; // Could be "*" for unregistered clients
        RequestedNick = parts[3].Split(' ')[0];
    }

    public override string Write()
    {
        return $":{ServerName} 433 {Nickname} {RequestedNick} :Nickname is already in use";
    }

    /// <summary>
    ///     Creates an ERR_NICKNAMEINUSE reply
    /// </summary>
    public static ErrNicknameInUse Create(string serverName, string nickname, string requestedNick)
    {
        return new ErrNicknameInUse
        {
            ServerName = serverName,
            Nickname = nickname,
            RequestedNick = requestedNick
        };
    }

    /// <summary>
    ///     Creates an ERR_NICKNAMEINUSE reply for an unregistered client
    /// </summary>
    public static ErrNicknameInUse CreateForUnregistered(string serverName, string requestedNick)
    {
        return new ErrNicknameInUse
        {
            ServerName = serverName,
            Nickname = "*", // Asterisk is used for unregistered clients
            RequestedNick = requestedNick
        };
    }
}
