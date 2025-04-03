using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
/// Represents an IRC PRIVMSG command used for sending messages to users or channels
/// </summary>
public class PrivMsgCommand : BaseIrcCommand
{
    /// <summary>
    /// The source of the message (user or server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The target of the message (user nickname or channel name)
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// The message content
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Indicates if the message is a CTCP request/response
    /// </summary>
    public bool IsCtcp => Message?.StartsWith('\u0001') == true && Message?.EndsWith('\u0001') == true;


    /// <summary>
    ///  Checks if the target is a channel message
    /// </summary>
    public bool IsChannelMessage => Target?.StartsWith('#') == true || Target?.StartsWith('&') == true;


    /// <summary>
    ///  Checks if the target is a user message
    /// </summary>
    public bool IsUserMessage => !IsChannelMessage && !string.IsNullOrEmpty(Target);


    /// <summary>
    /// Gets the CTCP command if this is a CTCP message
    /// </summary>
    public string CtcpCommand
    {
        get
        {
            if (!IsCtcp)
            {
                return null;
            }

            // Remove the \u0001 at start and end
            string content = Message.Substring(1, Message.Length - 2);
            int spacePos = content.IndexOf(' ');

            return spacePos > 0 ? content[..spacePos] : content;
        }
    }

    /// <summary>
    /// Gets the CTCP parameters if this is a CTCP message
    /// </summary>
    public string CtcpParameters
    {
        get
        {
            if (!IsCtcp)
            {
                return null;
            }

            // Remove the \u0001 at start and end
            string content = Message.Substring(1, Message.Length - 2);
            int spacePos = content.IndexOf(' ');

            return spacePos > 0 ? content[(spacePos + 1)..] : string.Empty;
        }
    }

    public PrivMsgCommand() : base("PRIVMSG")
    {
    }

    public PrivMsgCommand(string source, string target, string message) : base("PRIVMSG")
    {
        Source = source;
        Target = target;
        Message = message;
    }

    public override void Parse(string line)
    {
        // Reset existing data
        Source = null;
        Target = null;
        Message = null;

        // Check if line starts with a source prefix
        if (line.StartsWith(':'))
        {
            // Find the first space after the source
            int sourceEndIndex = line.IndexOf(' ');
            if (sourceEndIndex != -1)
            {
                Source = line.Substring(1, sourceEndIndex - 1);
                line = line.Substring(sourceEndIndex).TrimStart();
            }
        }

        // Tokenize remaining line
        var tokens = line.Split(new[] { ' ' }, 3);

        // Validate tokens
        if (tokens.Length < 3 || tokens[0].ToUpper() != "PRIVMSG")
        {
            throw new FormatException($"Invalid PRIVMSG format: {line}");
        }

        // Set target
        Target = tokens[1];

        // Extract message (remove leading : if present)
        Message = tokens[2].StartsWith(":")
            ? tokens[2].Substring(1)
            : tokens[2];
    }

    public override string Write()
    {
        if (!string.IsNullOrEmpty(Source))
        {
            return $":{Source} PRIVMSG {Target} :{Message}";
        }
        else
        {
            return $"PRIVMSG {Target} :{Message}";
        }
    }

    /// <summary>
    /// Creates a PRIVMSG from a user to a target
    /// </summary>
    public static PrivMsgCommand CreateFromUser(string userPrefix, string target, string message)
    {
        return new PrivMsgCommand
        {
            Source = userPrefix,
            Target = target,
            Message = message
        };
    }

    /// <summary>
    /// Creates a CTCP message (Client-To-Client Protocol)
    /// </summary>
    public static PrivMsgCommand CreateCtcp(string userPrefix, string target, string ctcpCommand, string parameters = null)
    {
        string ctcpMessage = string.IsNullOrEmpty(parameters)
            ? $"\u0001{ctcpCommand}\u0001"
            : $"\u0001{ctcpCommand} {parameters}\u0001";

        return new PrivMsgCommand
        {
            Source = userPrefix,
            Target = target,
            Message = ctcpMessage
        };
    }

    /// <summary>
    /// Creates an ACTION message (special CTCP message for describing actions)
    /// </summary>
    public static PrivMsgCommand CreateAction(string userPrefix, string target, string action)
    {
        return CreateCtcp(userPrefix, target, "ACTION", action);
    }

    /// <summary>
    /// Creates a PRIVMSG to a channel
    /// </summary>
    /// <param name="userPrefix">Source user's prefix (nick!user@host)</param>
    /// <param name="channel">Channel to send the message to</param>
    /// <param name="message">Message content</param>
    /// <returns>A new PrivMsgCommand for the channel message</returns>
    public static PrivMsgCommand CreateToChannel(string userPrefix, string channel, string message)
    {
        if (string.IsNullOrEmpty(channel) || !channel.StartsWith('#') && !channel.StartsWith('&'))
        {
            throw new ArgumentException("Channel must start with '#' or '&'", nameof(channel));
        }

        return new PrivMsgCommand
        {
            Source = userPrefix,
            Target = channel,
            Message = message
        };
    }

    /// <summary>
    /// Creates a CTCP message to a channel
    /// </summary>
    /// <param name="userPrefix">Source user's prefix (nick!user@host)</param>
    /// <param name="channel">Channel to send the CTCP message to</param>
    /// <param name="ctcpCommand">CTCP command</param>
    /// <param name="parameters">Optional CTCP parameters</param>
    /// <returns>A new PrivMsgCommand for the channel CTCP message</returns>
    public static PrivMsgCommand CreateCtcpToChannel(
        string userPrefix, string channel, string ctcpCommand, string parameters = null
    )
    {
        if (string.IsNullOrEmpty(channel) || !channel.StartsWith('#') && !channel.StartsWith('&'))
        {
            throw new ArgumentException("Channel must start with '#' or '&'", nameof(channel));
        }

        string ctcpMessage = string.IsNullOrEmpty(parameters)
            ? $"\u0001{ctcpCommand}\u0001"
            : $"\u0001{ctcpCommand} {parameters}\u0001";

        return new PrivMsgCommand
        {
            Source = userPrefix,
            Target = channel,
            Message = ctcpMessage
        };
    }

    /// <summary>
    /// Creates an ACTION message to a channel
    /// </summary>
    /// <param name="userPrefix">Source user's prefix (nick!user@host)</param>
    /// <param name="channel">Channel to send the action to</param>
    /// <param name="action">Action description</param>
    /// <returns>A new PrivMsgCommand for the channel action</returns>
    public static PrivMsgCommand CreateActionToChannel(string userPrefix, string channel, string action)
    {
        return CreateCtcpToChannel(userPrefix, channel, "ACTION", action);
    }
}
