using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC INVITE command used to invite users to a channel
/// </summary>
public class InviteCommand : BaseIrcCommand
{
    /// <summary>
    /// The user being invited
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The channel the user is being invited to
    /// </summary>
    public string Channel { get; set; }

    /// <summary>
    /// The user sending the invite (optional, used in server-to-client messages)
    /// </summary>
    public string Source { get; set; }

    public InviteCommand() : base("INVITE")
    {
    }

    public override void Parse(string line)
    {
        // Possible formats:
        // Client-to-server: INVITE <nickname> <channel>
        // Server-to-client: :source INVITE <nickname> <channel>

        if (line.StartsWith(':'))
        {
            // Server-to-client format
            var parts = line.Split(' ');

            if (parts.Length >= 4)
            {
                Source = parts[0].TrimStart(':');
                // parts[1] should be "INVITE"
                Nickname = parts[2];
                Channel = parts[3];
            }
        }
        else
        {
            // Client-to-server format
            var parts = line.Split(' ');

            if (parts.Length >= 3)
            {
                // parts[0] should be "INVITE"
                Nickname = parts[1];
                Channel = parts[2];
            }
        }
    }

    public override string Write()
    {
        if (!string.IsNullOrEmpty(Source))
        {
            // Server-to-client format
            return $":{Source} INVITE {Nickname} {Channel}";
        }
        else
        {
            // Client-to-server format
            return $"INVITE {Nickname} {Channel}";
        }
    }
}
