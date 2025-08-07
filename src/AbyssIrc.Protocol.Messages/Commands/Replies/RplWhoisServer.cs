using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_WHOISSERVER (312) numeric reply
///     Provides server information for a user in a WHOIS query
/// </summary>
public class RplWhoisServer : BaseIrcCommand
{
    public RplWhoisServer() : base("312")
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
    ///     The server the user is connected to
    /// </summary>
    public string UserServer { get; set; }

    /// <summary>
    ///     Additional information about the server connection
    /// </summary>
    public string ServerInfo { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 312 nickname targetuser irc.example.net :Server info description
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "312"
        Nickname = parts[2];
        QueriedNick = parts[3];
        UserServer = parts[4].Split(' ')[0];

        // Extract server info
        var colonIndex = line.IndexOf(':', parts[0].Length);
        if (colonIndex != -1)
        {
            ServerInfo = line.Substring(colonIndex + 1);
        }
    }

    public override string Write()
    {
        return string.IsNullOrEmpty(ServerInfo)
            ? $":{ServerName} 312 {Nickname} {QueriedNick} {UserServer}"
            : $":{ServerName} 312 {Nickname} {QueriedNick} {UserServer} :{ServerInfo}";
    }

    /// <summary>
    ///     Creates a RPL_WHOISSERVER reply
    /// </summary>
    public static RplWhoisServer Create(
        string serverName,
        string nickname,
        string queriedNick,
        string userServer,
        string serverInfo = null
    )
    {
        return new RplWhoisServer
        {
            ServerName = serverName,
            Nickname = nickname,
            QueriedNick = queriedNick,
            UserServer = userServer,
            ServerInfo = serverInfo
        };
    }
}
