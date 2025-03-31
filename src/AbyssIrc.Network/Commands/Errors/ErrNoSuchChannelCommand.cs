using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_NOSUCHCHANNEL (403) error response
///     Returned when a client tries to perform an operation on a channel that doesn't exist
/// </summary>
public class ErrNoSuchChannelCommand : BaseIrcCommand
{
    public ErrNoSuchChannelCommand() : base("403") => ErrorMessage = "No such channel";

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The channel name that doesn't exist
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_NOSUCHCHANNEL format: ":server 403 nickname channel :No such channel"

        if (!line.StartsWith(":"))
        {
            return; // Invalid format for server response
        }

        var parts = line.Split(' ', 5); // Maximum of 5 parts

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "403"
        Nickname = parts[2];
        ChannelName = parts[3];

        // Extract the error message (removes the leading ":")
        if (parts[4].StartsWith(":"))
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
        // Format: ":server 403 nickname channel :No such channel"
        return $":{ServerName} 403 {Nickname} {ChannelName} :{ErrorMessage}";
    }

    public static ErrNoSuchChannelCommand Create(string serverName, string nickname, string channelName)
    {
        return new ErrNoSuchChannelCommand
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName
        };
    }
}
