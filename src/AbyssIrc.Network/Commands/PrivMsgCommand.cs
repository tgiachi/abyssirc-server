using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

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
        // Examples:
        // :nick!user@host PRIVMSG #channel :Hello everyone!
        // :nick!user@host PRIVMSG target :\u0001ACTION waves\u0001
        // PRIVMSG #channel :Hello from client

        // Split the line into parts
        var parts = line.Split(' ', 3);

        if (parts[0].StartsWith(":"))
        {
            // Message with source/prefix
            Source = parts[0].TrimStart(':');

            if (parts.Length > 2)
            {
                Target = parts[1];

                // Extract message (might start with :)
                int colonPos = line.IndexOf(':', parts[0].Length);
                if (colonPos != -1)
                {
                    Message = line.Substring(colonPos + 1);
                }
            }
        }
        else
        {
            // Client-originated message without prefix
            // parts[0] should be "PRIVMSG"

            if (parts.Length > 2)
            {
                Target = parts[1];

                // Extract message (might start with :)
                int colonPos = line.IndexOf(':');
                if (colonPos != -1)
                {
                    Message = line.Substring(colonPos + 1);
                }
            }
        }
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
}
