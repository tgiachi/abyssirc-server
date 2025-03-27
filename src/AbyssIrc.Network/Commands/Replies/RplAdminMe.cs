using System.Text.RegularExpressions;
using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_ADMINME (256) numeric reply indicating server for admin info
/// </summary>
public class RplAdminMe : BaseIrcCommand
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
    /// The server being queried for admin info
    /// </summary>
    public string QueryServer { get; set; }

    public RplAdminMe() : base("256")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.com 256 nickname :Administrative info about server.com
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "256"
        Nickname = parts[2];

        // Extract server name from message
        string message = parts[3].TrimStart(':');
        var matches = Regex.Match(
            message,
            @"Administrative info about (.+)");

        if (matches.Success && matches.Groups.Count >= 2)
        {
            QueryServer = matches.Groups[1].Value;
        }
        else
        {
            QueryServer = ServerName; // Default to sending server
        }
    }

    public override string Write()
    {
        return $":{ServerName} 256 {Nickname} :Administrative info about {QueryServer}";
    }

    /// <summary>
    /// Creates an RPL_ADMINME reply
    /// </summary>
    public static RplAdminMe Create(string serverName, string nickname, string queryServer = null)
    {
        return new RplAdminMe
        {
            ServerName = serverName,
            Nickname = nickname,
            QueryServer = queryServer ?? serverName
        };
    }
}
