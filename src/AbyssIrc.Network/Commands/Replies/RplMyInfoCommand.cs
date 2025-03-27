using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     RPL_MYINFO (004) - Server version information
///     Format: :
///     <server>
///         004
///         <nickname>
///             <servername>
///                 <version>
///                     <available user modes>
///                         <available channel modes>
///                             Example: :irc.example.net 004 Mario irc.example.net ircd-2.11.2 aoOirw biklmnopstv
/// </summary>
public class RplMyInfoCommand : BaseIrcCommand
{
    public RplMyInfoCommand() : base("004")
    {
    }

    /// <summary>
    ///     Name of the IRC server
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     Version of the IRC server software
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    ///     Available user modes supported by the server
    /// </summary>
    public string UserModes { get; set; }

    /// <summary>
    ///     Available channel modes supported by the server
    /// </summary>
    public string ChannelModes { get; set; }

    /// <summary>
    ///     Target nickname receiving this message
    /// </summary>
    public string TargetNick { get; set; }

    /// <summary>
    ///     Parse a raw IRC message into this command
    /// </summary>
    public override void Parse(string rawMessage)
    {
        // Example: :irc.example.net 004 Mario irc.example.net ircd-2.11.2 aoOirw biklmnopstv
        var parts = rawMessage.Split(' ');

        if (parts.Length >= 6)
        {
            // Format: :<server> 004 <nickname> <servername> <version> <user modes> <channel modes>
            var prefix = parts[0].TrimStart(':');

            TargetNick = parts[2];
            ServerName = parts[3];
            Version = parts[4];
            UserModes = parts[5];

            // The channel modes might be split or combined in the rest of the message
            if (parts.Length >= 7)
            {
                ChannelModes = parts[6];
            }
        }
    }

    public override string Write()
    {
        return $":{ServerName} 004 {TargetNick} {ServerName} {Version} {UserModes} {ChannelModes}";
    }
}
