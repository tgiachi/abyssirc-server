using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
/// Represents ERR_INVITEONLYCHAN (473) numeric reply
/// Sent when a client tries to join an invite-only channel (mode +i) without being invited
/// Format: ":server 473 nickname #channel :Cannot join channel (+i) - you must be invited"
/// </summary>
public class ErrInviteOnlyChan : BaseIrcCommand
{
    public ErrInviteOnlyChan() : base("473")
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
    /// The channel name that is invite-only
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// The error message
    /// </summary>
    public string ErrorMessage { get; set; } = "Cannot join channel (+i) - you must be invited";

    public override void Parse(string line)
    {
        // Example: :irc.server.net 473 nickname #channel :Cannot join channel (+i) - you must be invited
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "473"
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
        return $":{ServerName} 473 {Nickname} {ChannelName} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_INVITEONLYCHAN reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="channelName">The channel name that is invite-only</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>A formatted ERR_INVITEONLYCHAN response</returns>
    public static ErrInviteOnlyChan Create(
        string serverName,
        string nickname,
        string channelName,
        string errorMessage = "Cannot join channel (+i) - you must be invited")
    {
        return new ErrInviteOnlyChan
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            ErrorMessage = errorMessage
        };
    }
}
