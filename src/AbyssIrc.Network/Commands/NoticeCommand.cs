using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents an IRC NOTICE command used for sending notices that don't require acknowledgment
/// </summary>
public class NoticeCommand : BaseIrcCommand
{
    /// <summary>
    /// The source/prefix of the notice (typically the server or user sending it)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The target of the notice (can be a nickname, channel, or special target like AUTH)
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// The message content of the notice
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Indicates if this is a server notice
    /// </summary>
    public bool IsServerNotice => !string.IsNullOrEmpty(Source) && !Source.Contains("!");

    public NoticeCommand() : base("NOTICE")
    {
    }

    public override void Parse(string line)
    {
        // Examples:
        // Server notice: :bitcoin.uk.eu.dal.net NOTICE AUTH :*** Looking up your hostname..
        // User notice: :nick!user@host NOTICE #channel :This is a notice
        // Client notice: NOTICE nickname :This is a direct notice

        // Split into parts
        var parts = line.Split(' ', 3);

        if (parts.Length < 3)
            return; // Invalid format

        // Check if there's a source/prefix
        if (parts[0].StartsWith(":"))
        {
            Source = parts[0].TrimStart(':');
            // parts[1] should be "NOTICE"
            Target = parts[2].Split(' ')[0]; // Get the target (first word of the third part)

            // Extract the message
            int messageStart = line.IndexOf(':', parts[0].Length);
            if (messageStart != -1)
            {
                Message = line.Substring(messageStart + 1);
            }
        }
        else
        {
            // Client request format without prefix
            // parts[0] should be "NOTICE"
            Target = parts[1];

            // Extract the message
            int messageStart = line.IndexOf(':', parts[0].Length);
            if (messageStart != -1)
            {
                Message = line.Substring(messageStart + 1);
            }
        }
    }

    public override string Write()
    {
        if (!string.IsNullOrEmpty(Source))
        {
            // With source prefix
            return $":{Source} NOTICE {Target} :{Message}";
        }
        else
        {
            // Client request format
            return $"NOTICE {Target} :{Message}";
        }
    }
}
