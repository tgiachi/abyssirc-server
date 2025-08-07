using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents ERR_TOOMANYCHANNELS (405) numeric reply
///     Sent when a client tries to join a channel but is already in too many channels
///     Format: ":server 405 nickname #channel :You have joined too many channels"
/// </summary>
public class ErrTooManyChannels : BaseIrcCommand
{
    public ErrTooManyChannels() : base("405")
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
    ///     The channel name that the client attempted to join
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The error message
    /// </summary>
    public string ErrorMessage { get; set; } = "You have joined too many channels";

    public override void Parse(string line)
    {
        // Example: :irc.server.net 405 nickname #channel :You have joined too many channels
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "405"
        Nickname = parts[2];
        ChannelName = parts[3];

        // Extract error message (removes the leading ":")
        if (parts[4].StartsWith(':'))
        {
            ErrorMessage = parts[4].Substring(1);
        }
        else
        {
            ErrorMessage = parts[4];
        }
    }

    public override string Write()
    {
        return $":{ServerName} 405 {Nickname} {ChannelName} :{ErrorMessage}";
    }

    /// <summary>
    ///     Creates an ERR_TOOMANYCHANNELS reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="channelName">The channel name the client tried to join</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>A formatted ERR_TOOMANYCHANNELS response</returns>
    public static ErrTooManyChannels Create(
        string serverName,
        string nickname,
        string channelName,
        string errorMessage = "You have joined too many channels"
    )
    {
        return new ErrTooManyChannels
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            ErrorMessage = errorMessage
        };
    }
}
