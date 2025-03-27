using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_ADMINEMAIL (259) numeric reply showing admin email contact
/// </summary>
public class RplAdminEmail : BaseIrcCommand
{
    public RplAdminEmail() : base("259")
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
    ///     The email contact for the server admin
    /// </summary>
    public string EmailAddress { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 259 nickname :admin@example.com
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "259"
        Nickname = parts[2];
        EmailAddress = parts[3].TrimStart(':');
    }

    public override string Write()
    {
        return $":{ServerName} 259 {Nickname} :{EmailAddress}";
    }

    /// <summary>
    ///     Creates an RPL_ADMINEMAIL reply
    /// </summary>
    public static RplAdminEmail Create(string serverName, string nickname, string emailAddress)
    {
        return new RplAdminEmail
        {
            ServerName = serverName,
            Nickname = nickname,
            EmailAddress = emailAddress
        };
    }
}
