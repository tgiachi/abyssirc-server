using System.Text.RegularExpressions;
using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_LUSERCLIENT (251) numeric reply showing user statistics
/// </summary>
public class RplLuserClient : BaseIrcCommand
{
    public RplLuserClient() : base("251")
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
    ///     Number of visible users
    /// </summary>
    public int VisibleUsers { get; set; }

    /// <summary>
    ///     Number of invisible users
    /// </summary>
    public int InvisibleUsers { get; set; }

    /// <summary>
    ///     Number of servers
    /// </summary>
    public int Servers { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 251 nickname :There are 3 users and 1 invisible on 2 servers
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "251"
        Nickname = parts[2];

        // Parse the message for user and server counts
        var message = parts[3].TrimStart(':');

        // This is a simplified parse - in a real implementation you'd want more robust parsing
        try
        {
            var matches = Regex.Match(
                message,
                @"There are (\d+) users and (\d+) invisible on (\d+) servers"
            );

            if (matches.Success && matches.Groups.Count >= 4)
            {
                VisibleUsers = int.Parse(matches.Groups[1].Value);
                InvisibleUsers = int.Parse(matches.Groups[2].Value);
                Servers = int.Parse(matches.Groups[3].Value);
            }
        }
        catch
        {
            // Parsing failed, but we can still use the message
        }
    }

    public override string Write()
    {
        return
            $":{ServerName} 251 {Nickname} :There are {VisibleUsers} users and {InvisibleUsers} invisible on {Servers} servers";
    }

    /// <summary>
    ///     Creates an RPL_LUSERCLIENT reply
    /// </summary>
    public static RplLuserClient Create(
        string serverName, string nickname, int visibleUsers, int invisibleUsers, int servers
    )
    {
        return new RplLuserClient
        {
            ServerName = serverName,
            Nickname = nickname,
            VisibleUsers = visibleUsers,
            InvisibleUsers = invisibleUsers,
            Servers = servers
        };
    }
}
