using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_WHOREPLY (352) numeric reply
///     Returns a line of information about a particular user matching the WHO query
///     Format: ":server 352 nickname channel username hostname server nick flags :hopcount realname"
/// </summary>
public class RplWhoReply : BaseIrcCommand
{
    public RplWhoReply() : base("352")
    {
    }

    /// <summary>
    ///     The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The channel the user is on (or "*" if not specific to a channel)
    /// </summary>
    public string Channel { get; set; }

    /// <summary>
    ///     The username of the target user
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    ///     The hostname of the target user
    /// </summary>
    public string Hostname { get; set; }

    /// <summary>
    ///     The server the target user is connected to
    /// </summary>
    public string UserServer { get; set; }

    /// <summary>
    ///     The nickname of the target user
    /// </summary>
    public string UserNick { get; set; }

    /// <summary>
    ///     Flags indicating user status (H=here, G=gone, *=IRC operator, @=channel operator, etc.)
    /// </summary>
    public string Flags { get; set; }

    /// <summary>
    ///     The hop count (distance in server hops)
    /// </summary>
    public int HopCount { get; set; }

    /// <summary>
    ///     The real name (or portion of it) of the target user
    /// </summary>
    public string RealName { get; set; }

    public override void Parse(string line)
    {
        // Example: :irc.server.net 352 requester #channel user host irc.server.net nick H :0 Real Name
        var parts = line.Split(' ');

        if (parts.Length < 9)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "352"
        Nickname = parts[2];
        Channel = parts[3];
        Username = parts[4];
        Hostname = parts[5];
        UserServer = parts[6];
        UserNick = parts[7];
        Flags = parts[8];

        // Extract hopcount and realname (they're in the trailing parameter, after the colon)
        var colonIndex = line.IndexOf(':', parts[0].Length);
        if (colonIndex != -1)
        {
            var trailingPart = line.Substring(colonIndex + 1).Trim();
            var spaceIndex = trailingPart.IndexOf(' ');

            if (spaceIndex != -1 && int.TryParse(trailingPart.Substring(0, spaceIndex), out var hopCount))
            {
                HopCount = hopCount;
                RealName = trailingPart.Substring(spaceIndex + 1);
            }
            else
            {
                // If we can't parse the hopcount properly, store the whole trailing part as realname
                RealName = trailingPart;
            }
        }
    }

    public override string Write()
    {
        return
            $":{ServerName} 352 {Nickname} {Channel} {Username} {Hostname} {UserServer} {UserNick} {Flags} :{HopCount} {RealName}";
    }

    /// <summary>
    ///     Creates an RPL_WHOREPLY response with complete information
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">Nickname of the client that sent the WHO request</param>
    /// <param name="channel">Channel the user is on (or "*" if not channel specific)</param>
    /// <param name="username">Username of the target</param>
    /// <param name="hostname">Hostname of the target</param>
    /// <param name="userServer">Server the target is connected to</param>
    /// <param name="userNick">Nickname of the target</param>
    /// <param name="flags">Status flags (H/G, operator status, etc.)</param>
    /// <param name="hopCount">Server hop count</param>
    /// <param name="realName">Real name of the target</param>
    /// <returns>A formatted RPL_WHOREPLY response</returns>
    public static RplWhoReply Create(
        string serverName,
        string nickname,
        string channel,
        string username,
        string hostname,
        string userServer,
        string userNick,
        string flags,
        int hopCount,
        string realName
    )
    {
        return new RplWhoReply
        {
            ServerName = serverName,
            Nickname = nickname,
            Channel = channel,
            Username = username,
            Hostname = hostname,
            UserServer = userServer,
            UserNick = userNick,
            Flags = flags,
            HopCount = hopCount,
            RealName = realName
        };
    }

    /// <summary>
    ///     Creates an RPL_WHOREPLY response with common status flags
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">Nickname of the client that sent the WHO request</param>
    /// <param name="channel">Channel the user is on (or "*" if not channel specific)</param>
    /// <param name="username">Username of the target</param>
    /// <param name="hostname">Hostname of the target</param>
    /// <param name="userNick">Nickname of the target</param>
    /// <param name="isAway">Whether the user is away</param>
    /// <param name="isOperator">Whether the user is an IRC operator</param>
    /// <param name="isChannelOperator">Whether the user is a channel operator</param>
    /// <param name="realName">Real name of the target</param>
    /// <returns>A formatted RPL_WHOREPLY response with appropriate flags</returns>
    public static RplWhoReply CreateWithFlags(
        string serverName,
        string nickname,
        string channel,
        string username,
        string hostname,
        string userNick,
        bool isAway = false,
        bool isOperator = false,
        bool isChannelOperator = false,
        string realName = ""
    )
    {
        // Build the flags string
        var flags = isAway ? "G" : "H"; // H=here, G=gone

        if (isOperator)
        {
            flags += "*"; // * for IRC operators
        }

        if (isChannelOperator && channel.StartsWith("#"))
        {
            flags += "@"; // @ for channel operators
        }

        return Create(
            serverName,
            nickname,
            channel,
            username,
            hostname,
            serverName, // Using the same server name for user's server
            userNick,
            flags,
            0, // Hop count is typically 0 for users on the same server
            realName
        );
    }
}
