using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
/// Represents ERR_CHANNELISFULL (471) numeric reply
/// Sent when a client tries to join a channel that has reached its user limit (mode +l)
/// Format: ":server 471 nickname #channel :Cannot join channel (+l) - channel is full"
/// </summary>
public class ErrChannelIsFull : BaseIrcCommand
{
    public ErrChannelIsFull() : base("471")
    {
    }

    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The channel name that is full
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// The error message
    /// </summary>
    public string ErrorMessage { get; set; } = "Cannot join channel (+l) - channel is full";

    public override void Parse(string line)
    {
        // Example: :irc.server.net 471 nickname #channel :Cannot join channel (+l) - channel is full
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "471"
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
        return $":{ServerName} 471 {Nickname} {ChannelName} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_CHANNELISFULL reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="channelName">The channel name that is full</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>A formatted ERR_CHANNELISFULL response</returns>
    public static ErrChannelIsFull Create(
        string serverName,
        string nickname,
        string channelName,
        string errorMessage = "Cannot join channel (+l) - channel is full")
    {
        return new ErrChannelIsFull
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            ErrorMessage = errorMessage
        };
    }
}
