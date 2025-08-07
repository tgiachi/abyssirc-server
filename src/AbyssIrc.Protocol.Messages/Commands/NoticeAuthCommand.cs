using AbyssIrc.Protocol.Messages.Types;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Specialized implementation for handling server NOTICE AUTH messages during connection
/// </summary>
public class NoticeAuthCommand : NoticeCommand
{
    public NoticeAuthCommand() =>
        // Override target to be specifically AUTH
        Target = "AUTH";

    /// <summary>
    ///     The type of AUTH notice (e.g. hostname lookup, ident check)
    /// </summary>
    public NoticeAuthType NoticeType { get; set; }

    /// <summary>
    ///     The server name sending the notice
    /// </summary>
    public string ServerName => Source;

    public override void Parse(string line)
    {
        // Call the base implementation first
        base.Parse(line);

        // Ensure this is actually an AUTH notice
        if (Target != "AUTH")
        {
            return;
        }

        // Determine the type of AUTH notice based on the message content
        if (Message.Contains("Looking up your hostname"))
        {
            NoticeType = NoticeAuthType.HostnameLookup;
        }
        else if (Message.Contains("Found your hostname"))
        {
            NoticeType = NoticeAuthType.HostnameFound;
        }
        else if (Message.Contains("Checking Ident"))
        {
            NoticeType = NoticeAuthType.IdentCheck;
        }
        else if (Message.Contains("No ident response"))
        {
            NoticeType = NoticeAuthType.NoIdent;
        }
        else if (Message.Contains("You are not authorized"))
        {
            NoticeType = NoticeAuthType.Unauthorized;
        }
        else
        {
            NoticeType = NoticeAuthType.Other;
        }
    }

    /// <summary>
    ///     Create a server NOTICE AUTH message
    /// </summary>
    /// <param name="serverName">The name of the server</param>
    /// <param name="message">The message to send</param>
    /// <param name="noticeType">The type of notice</param>
    /// <returns>A configured NoticeAuthCommand</returns>
    public static NoticeAuthCommand Create(
        string serverName, string message, NoticeAuthType noticeType = NoticeAuthType.Other
    )
    {
        return new NoticeAuthCommand
        {
            Source = serverName,
            Target = "AUTH",
            Message = message.StartsWith("*** ") ? message : $"*** {message}",
            NoticeType = noticeType
        };
    }
}
