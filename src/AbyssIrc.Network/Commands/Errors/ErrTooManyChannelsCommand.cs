using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_TOOMANYCHANNELS (405) error response
///     Returned when a client tries to join a channel but is already a member of too many channels
/// </summary>
public class ErrTooManyChannelsCommand : BaseIrcCommand
{
    public ErrTooManyChannelsCommand() : base("405") => ErrorMessage = "You have joined too many channels";

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The channel name that the user attempted to join
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_TOOMANYCHANNELS format: ":server 405 nickname channel :You have joined too many channels"

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
        // parts[1] should be "405"
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
        // Format: ":server 405 nickname channel :You have joined too many channels"
        return $":{ServerName} 405 {Nickname} {ChannelName} :{ErrorMessage}";
    }
}
