using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_AWAY (301) numeric reply
/// </summary>
public class RplAway : BaseIrcCommand
{
    public RplAway() : base("301")
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
    ///     The nickname of the away user
    /// </summary>
    public string AwayNick { get; set; }

    /// <summary>
    ///     The away message
    /// </summary>
    public string AwayMessage { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 301 nickname awaynick :Gone to lunch
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "301"
        Nickname = parts[2];
        AwayNick = parts[3].Split(' ')[0];

        // Extract away message
        var messageStart = line.IndexOf(':', parts[0].Length);
        if (messageStart != -1)
        {
            AwayMessage = line.Substring(messageStart + 1);
        }
    }

    public override string Write()
    {
        return $":{ServerName} {Code} {Nickname} {AwayNick} :{AwayMessage}";
    }

    /// <summary>
    ///     Creates a RPL_AWAY reply
    /// </summary>
    public static RplAway Create(string serverName, string nickname, string awayNick, string awayMessage)
    {
        return new RplAway
        {
            ServerName = serverName,
            Nickname = nickname,
            AwayNick = awayNick,
            AwayMessage = awayMessage
        };
    }
}
