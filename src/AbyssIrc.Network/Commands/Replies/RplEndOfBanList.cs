using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_ENDOFBANLIST (368) numeric reply
/// Indicates the end of a ban list
/// Format: ":server 368 nickname #channel :End of channel ban list"
/// </summary>
public class RplEndOfBanList : BaseIrcCommand
{
    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The channel name
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// The custom message (typically "End of channel ban list")
    /// </summary>
    public string Message { get; set; } = "End of channel ban list";

    public RplEndOfBanList() : base("368")
    {
    }

    public override void Parse(string line)
    {
        // Example: :irc.server.net 368 nickname #channel :End of channel ban list
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "368"
        Nickname = parts[2];
        ChannelName = parts[3];

        // Extract message (removes the leading ":")
        if (parts[4].StartsWith(':'))
        {
            Message = parts[4].Substring(1);
        }
        else
        {
            Message = parts[4];
        }
    }

    public override string Write()
    {
        return $":{ServerName} 368 {Nickname} {ChannelName} :{Message}";
    }

    /// <summary>
    /// Creates a RPL_ENDOFBANLIST reply
    /// </summary>
    public static RplEndOfBanList Create(
        string serverName,
        string nickname,
        string channelName,
        string message = "End of channel ban list")
    {
        return new RplEndOfBanList
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            Message = message
        };
    }
}
