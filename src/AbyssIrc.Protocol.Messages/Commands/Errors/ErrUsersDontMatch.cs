using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents ERR_USERSDONTMATCH (502) numeric reply
///     Sent when a client tries to change the mode for another user
/// </summary>
public class ErrUsersDontMatch : BaseIrcCommand
{
    public ErrUsersDontMatch() : base("502")
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
        // Example: :server.com 502 nickname :Cannot change mode for other users
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "502"
        Nickname = parts[2];
    }

    public override string Write()
    {
        return $":{ServerName} 502 {Nickname} :Cannot change mode for other users";
    }

    /// <summary>
    ///     Creates an ERR_USERSDONTMATCH reply
    /// </summary>
    public static ErrUsersDontMatch Create(string serverName, string nickname)
    {
        return new ErrUsersDontMatch
        {
            ServerName = serverName,
            Nickname = nickname
        };
    }
}
