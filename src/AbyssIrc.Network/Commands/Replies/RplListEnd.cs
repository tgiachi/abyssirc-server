using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_LISTEND (323) numeric reply that marks the end of LIST response
/// </summary>
public class RplListEnd : BaseIrcCommand
{
    public RplListEnd() : base("323")
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
        // Example: :server.com 323 nickname :End of /LIST
        var parts = line.Split(' ', 4);

        if (parts.Length < 3)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "323"
        Nickname = parts[2];
    }

    public override string Write()
    {
        return $":{ServerName} 323 {Nickname} :End of /LIST";
    }

    /// <summary>
    ///     Creates a RPL_LISTEND reply
    /// </summary>
    public static RplListEnd Create(string serverName, string nickname)
    {
        return new RplListEnd
        {
            ServerName = serverName,
            Nickname = nickname
        };
    }
}
