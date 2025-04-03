using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_LISTSTART (321) numeric reply that marks the start of LIST response
/// </summary>
public class RplListStart : BaseIrcCommand
{
    public RplListStart() : base("321")
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
        // Example: :server.com 321 nickname :Channel Users Name
        var parts = line.Split(' ', 4);

        if (parts.Length < 3)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "321"
        Nickname = parts[2];
    }

    public override string Write()
    {
        return $":{ServerName} 321 {Nickname} :Channel Users Name";
    }

    /// <summary>
    ///     Creates a RPL_LISTSTART reply
    /// </summary>
    public static RplListStart Create(string serverName, string nickname)
    {
        return new RplListStart
        {
            ServerName = serverName,
            Nickname = nickname
        };
    }
}
