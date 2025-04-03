using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_WHOISACCOUNT (330) numeric reply that shows a user's services account
/// </summary>
public class RplWhoisAccount : BaseIrcCommand
{
    public RplWhoisAccount() : base("330")
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
    ///     The queried nickname for WHOIS
    /// </summary>
    public string QueriedNick { get; set; }

    /// <summary>
    ///     The services account name
    /// </summary>
    public string AccountName { get; set; }

    /// <summary>
    ///     The explanatory message
    /// </summary>
    public string Message { get; set; } = "is logged in as";

    public override void Parse(string line)
    {
        // Example: :server.com 330 nickname targetuser accountname :is logged in as
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "330"
        Nickname = parts[2];
        QueriedNick = parts[3];

        // Account name and message may be combined
        var remainder = parts[4];
        var colonPos = remainder.IndexOf(':');

        if (colonPos != -1)
        {
            AccountName = remainder.Substring(0, colonPos).Trim();
            Message = remainder.Substring(colonPos + 1).Trim();
        }
        else
        {
            AccountName = remainder.Trim();
        }
    }

    public override string Write()
    {
        return $":{ServerName} 330 {Nickname} {QueriedNick} {AccountName} :{Message}";
    }

    /// <summary>
    ///     Creates a RPL_WHOISACCOUNT reply
    /// </summary>
    public static RplWhoisAccount Create(
        string serverName, string nickname, string queriedNick,
        string accountName, string message = "is logged in as"
    )
    {
        return new RplWhoisAccount
        {
            ServerName = serverName,
            Nickname = nickname,
            QueriedNick = queriedNick,
            AccountName = accountName,
            Message = message
        };
    }
}
