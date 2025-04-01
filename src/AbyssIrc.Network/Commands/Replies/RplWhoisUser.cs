using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_WHOISUSER (311) numeric reply
///     Provides details about a user in a WHOIS query
/// </summary>
public class RplWhoisUser : BaseIrcCommand
{
    public RplWhoisUser() : base("311")
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
    ///     The nickname being queried
    /// </summary>
    public string QueriedNick { get; set; }

    /// <summary>
    ///     The username (ident) of the queried user
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    ///     The hostname of the queried user
    /// </summary>
    public string Hostname { get; set; }

    /// <summary>
    ///     The real name (gecos) of the queried user
    /// </summary>
    public string RealName { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 311 nickname targetuser username hostname * :Real Name
        var parts = line.Split(' ', 6);

        if (parts.Length < 6)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "311"
        Nickname = parts[2];
        QueriedNick = parts[3];
        Username = parts[4];
        Hostname = parts[5].Split(' ')[0];

        // Extract real name
        var colonIndex = line.IndexOf(':', parts[0].Length);
        if (colonIndex != -1)
        {
            RealName = line.Substring(colonIndex + 1);
        }
    }

    public override string Write()
    {
        return $":{ServerName} 311 {Nickname} {QueriedNick} {Username} {Hostname} * :{RealName}";
    }

    /// <summary>
    ///     Creates a RPL_WHOISUSER reply
    /// </summary>
    public static RplWhoisUser Create(
        string serverName,
        string nickname,
        string queriedNick,
        string username,
        string hostname,
        string realName
    )
    {
        return new RplWhoisUser
        {
            ServerName = serverName,
            Nickname = nickname,
            QueriedNick = queriedNick,
            Username = username,
            Hostname = hostname,
            RealName = realName
        };
    }
}
