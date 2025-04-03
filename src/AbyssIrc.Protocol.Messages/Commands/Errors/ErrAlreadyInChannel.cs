using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
/// Represents an IRC ERR_USERONCHANNEL (443) error response
/// Returned when a user tries to invite or add a user to a channel they are already in
/// </summary>
public class ErrAlreadyInChannel : BaseIrcCommand
{
    public ErrAlreadyInChannel() : base("443") => ErrorMessage = "User is already in channel";

    /// <summary>
    /// The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The nickname of the user who is already in the channel
    /// </summary>
    public string UserNickname { get; set; }

    /// <summary>
    /// The name of the channel
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_USERONCHANNEL format: ":server 443 nickname targetuser #channel :is already on channel"

        if (!line.StartsWith(':'))
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
        UserNickname = parts[3];
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
        // Format: ":server 443 nickname targetuser #channel :is already on channel"
        return $":{ServerName} 443 {Nickname} {UserNickname} {ChannelName} :{ErrorMessage}";
    }

    /// <summary>
    /// Creates an ERR_USERONCHANNEL (443) reply
    /// </summary>
    public static ErrAlreadyInChannel Create(
        string serverName,
        string nickname,
        string userNickname,
        string channelName,
        string errorMessage = null
    )
    {
        return new ErrAlreadyInChannel
        {
            ServerName = serverName,
            Nickname = nickname,
            UserNickname = userNickname,
            ChannelName = channelName,
            ErrorMessage = errorMessage ?? "is already on channel"
        };
    }
}
