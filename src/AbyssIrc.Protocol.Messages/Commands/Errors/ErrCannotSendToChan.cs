using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents ERR_CANNOTSENDTOCHAN (404) numeric reply
/// </summary>
public class ErrCannotSendToChan : BaseIrcCommand
{
    public ErrCannotSendToChan() : base("404")
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
    ///     The channel name that cannot be sent to
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     Optional reason why the message cannot be sent
    /// </summary>
    public string Reason { get; set; } = "Cannot send to channel";

    public override void Parse(string line)
    {
        // Example: :server.com 404 nickname #channel :Cannot send to channel
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "404"
        Nickname = parts[2];
        ChannelName = parts[3].Split(' ')[0];

        // Extract reason if present
        var reasonStart = line.IndexOf(':', parts[0].Length);
        if (reasonStart != -1)
        {
            Reason = line.Substring(reasonStart + 1);
        }
    }

    public override string Write()
    {
        return $":{ServerName} {Code} {Nickname} {ChannelName} :{Reason}";
    }

    /// <summary>
    ///     Creates an ERR_CANNOTSENDTOCHAN reply
    /// </summary>
    public static ErrCannotSendToChan Create(string serverName, string nickname, string channelName, string reason = null)
    {
        return new ErrCannotSendToChan
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            Reason = reason ?? "Cannot send to channel"
        };
    }
}
