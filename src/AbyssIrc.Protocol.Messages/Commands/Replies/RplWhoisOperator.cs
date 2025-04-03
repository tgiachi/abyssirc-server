using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_WHOISOPERATOR (313) numeric reply
///     Indicates if the user is an IRC operator
/// </summary>
public class RplWhoisOperator : BaseIrcCommand
{
    public RplWhoisOperator() : base("313")
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
    ///     The message indicating operator status
    /// </summary>
    public string Message { get; set; } = "is an IRC operator";

    public override void Parse(string line)
    {
        // Example: :server.com 313 nickname targetuser :is an IRC operator
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "313"
        Nickname = parts[2];
        QueriedNick = parts[3];

        // Extract message
        var colonIndex = line.IndexOf(':', parts[0].Length);
        if (colonIndex != -1)
        {
            Message = line.Substring(colonIndex + 1);
        }
    }

    public override string Write()
    {
        return $":{ServerName} 313 {Nickname} {QueriedNick} :{Message}";
    }

    /// <summary>
    ///     Creates a RPL_WHOISOPERATOR reply
    /// </summary>
    public static RplWhoisOperator Create(
        string serverName,
        string nickname,
        string queriedNick,
        string message = "is an IRC operator"
    )
    {
        return new RplWhoisOperator
        {
            ServerName = serverName,
            Nickname = nickname,
            QueriedNick = queriedNick,
            Message = message
        };
    }
}
