using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC QUIT command used when a client disconnects from the server
/// </summary>
public class QuitCommand : BaseIrcCommand
{
    /// <summary>
    /// The quit message/reason provided by the client
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The source/prefix of the command (typically set in quit notifications)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Indicates if this is a notification from the server
    /// </summary>
    public bool IsNotification { get; set; }

    public QuitCommand() : base("QUIT")
    {
    }

    public override void Parse(string line)
    {
        // Examples:
        // Client disconnect: QUIT :Leaving
        // Server notification: :nick!user@host QUIT :Client Quit

        // Handle server notification format
        if (line.StartsWith(":"))
        {
            IsNotification = true;

            // Split into parts
            var parts = line.Split(' ', 3);

            if (parts.Length < 2)
                return; // Invalid format

            Source = parts[0].TrimStart(':');
            // parts[1] should be "QUIT"

            // Get the quit message if provided
            if (parts.Length > 2)
            {
                string message = parts[2];
                if (message.StartsWith(":"))
                    message = message.Substring(1);

                Message = message;
            }
        }
        else
        {
            // Client request format
            var parts = line.Split(' ', 2);

            // parts[0] should be "QUIT"

            // Get the quit message if provided
            if (parts.Length > 1)
            {
                string message = parts[1];
                if (message.StartsWith(":"))
                    message = message.Substring(1);

                Message = message;
            }
        }
    }

    public override string Write()
    {
        if (IsNotification && !string.IsNullOrEmpty(Source))
        {
            // Notification format
            return string.IsNullOrEmpty(Message)
                ? $":{Source} QUIT"
                : $":{Source} QUIT :{Message}";
        }
        else
        {
            // Client request format
            return string.IsNullOrEmpty(Message)
                ? "QUIT"
                : $"QUIT :{Message}";
        }
    }
}
