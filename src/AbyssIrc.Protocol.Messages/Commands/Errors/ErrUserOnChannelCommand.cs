using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_USERONCHANNEL (443) error response
///     Returned when trying to invite a user to a channel they are already on
/// </summary>
public class ErrUserOnChannelCommand : BaseIrcCommand
{
    public ErrUserOnChannelCommand() : base("443") => ErrorMessage = "is already on channel";

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The nickname of the user already on the channel
    /// </summary>
    public string UserNick { get; set; }

    /// <summary>
    ///     The channel name
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_USERONCHANNEL format: ":server 443 nickname user #channel :is already on channel"

        if (!line.StartsWith(":"))
        {
            return; // Invalid format for server response
        }

        var parts = line.Split(' ', 6); // Maximum of 6 parts

        if (parts.Length < 6)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "443"
        Nickname = parts[2];
        UserNick = parts[3];
        ChannelName = parts[4];

        // Extract the error message (removes the leading ":")
        if (parts[5].StartsWith(":"))
        {
            ErrorMessage = parts[5].Substring(1);
        }
        else
        {
            ErrorMessage = parts[5];
        }
    }

    public override string Write()
    {
        // Format: ":server 443 nickname user #channel :is already on channel"
        return $":{ServerName} 443 {Nickname} {UserNick} {ChannelName} :{ErrorMessage}";
    }

    /// <summary>
    ///     Creates an ERR_USERONCHANNEL error response
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target user nickname</param>
    /// <param name="userNick">The nickname of the user already on the channel</param>
    /// <param name="channelName">The channel name</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>The constructed ERR_USERONCHANNEL command</returns>
    public static ErrUserOnChannelCommand Create(
        string serverName,
        string nickname,
        string userNick,
        string channelName,
        string errorMessage = "is already on channel"
    )
    {
        return new ErrUserOnChannelCommand
        {
            ServerName = serverName,
            Nickname = nickname,
            UserNick = userNick,
            ChannelName = channelName,
            ErrorMessage = errorMessage
        };
    }
}
