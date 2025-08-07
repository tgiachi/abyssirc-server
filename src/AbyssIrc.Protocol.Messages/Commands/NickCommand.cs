using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents an IRC NICK command used to set or change a user's nickname
/// </summary>
public class NickCommand : BaseIrcCommand
{
    public NickCommand() : base("NICK")
    {
    }

    /// <summary>
    ///     The nickname requested by the client or being changed to
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The source/prefix of the command (typically set in nickname change notifications)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     Indicates if this is a change notification from the server
    /// </summary>
    public bool IsChangeNotification { get; set; }

    public override void Parse(string line)
    {
        // Examples:
        // Client initial: NICK Guest82
        // Change notification: :oldnick!user@host NICK :newnick

        // Handle server/change notification format
        if (line.StartsWith(":"))
        {
            IsChangeNotification = true;

            // Split into parts
            var parts = line.Split(' ');

            if (parts.Length < 3)
            {
                return; // Invalid format
            }

            Source = parts[0].TrimStart(':');
            // parts[1] should be "NICK"

            // The new nickname might be prefixed with : if it's the last parameter
            var newNick = parts[2];
            if (newNick.StartsWith(":"))
            {
                newNick = newNick.Substring(1);
            }

            Nickname = newNick;
        }
        else
        {
            // Client request format
            var parts = line.Split(' ');

            if (parts.Length < 2)
            {
                return; // Invalid format
            }

            // parts[0] should be "NICK"

            // The nickname might be prefixed with : but typically isn't for the initial NICK command
            var nick = parts[1];
            if (nick.StartsWith(":"))
            {
                nick = nick.Substring(1);
            }

            Nickname = nick;
        }
    }

    public override string Write()
    {
        if (IsChangeNotification && !string.IsNullOrEmpty(Source))
        {
            // Change notification format
            return $":{Source} NICK :{Nickname}";
        }

        // Client request format
        return $"NICK {Nickname}";
    }
}
