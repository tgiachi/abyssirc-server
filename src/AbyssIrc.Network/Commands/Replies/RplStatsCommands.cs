using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents RPL_STATSCOMMANDS (212) numeric reply used to list command usage statistics
/// </summary>
public class RplStatsCommands : BaseIrcCommand
{
    public RplStatsCommands() : base("212")
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
    ///     The command name
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    ///     Count of uses of this command
    /// </summary>
    public long Count { get; set; }

    /// <summary>
    ///     Bytes used by this command
    /// </summary>
    public long ByteCount { get; set; }

    /// <summary>
    ///     Remote count (from clients)
    /// </summary>
    public long RemoteCount { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 212 nickname PRIVMSG 1234 65432 543
        var parts = line.Split(' ');

        if (parts.Length < 6)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "212"
        Nickname = parts[2];
        Command = parts[3];

        // Parse numeric values
        if (long.TryParse(parts[4], out var count))
        {
            Count = count;
        }

        if (long.TryParse(parts[5], out var byteCount))
        {
            ByteCount = byteCount;
        }

        if (parts.Length > 6 && long.TryParse(parts[6], out var remoteCount))
        {
            RemoteCount = remoteCount;
        }
    }

    public override string Write()
    {
        return $":{ServerName} 212 {Nickname} {Command} {Count} {ByteCount} {RemoteCount}";
    }

    /// <summary>
    ///     Creates an RPL_STATSCOMMANDS reply
    /// </summary>
    public static RplStatsCommands Create(
        string serverName, string nickname, string command, long count, long byteCount, long remoteCount = 0
    )
    {
        return new RplStatsCommands
        {
            ServerName = serverName,
            Nickname = nickname,
            Command = command,
            Count = count,
            ByteCount = byteCount,
            RemoteCount = remoteCount
        };
    }
}
