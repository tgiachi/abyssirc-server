using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_WELCOME (001) numeric reply
///     This is the first message sent after client registration is complete
///     Format: ":server 001 nickname :Welcome to the Internet Relay Network nickname!user@host"
/// </summary>
public class RplWelcome : BaseIrcCommand
{
    public RplWelcome() : base("001")
    {
    }

    /// <summary>
    ///     The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The network name
    /// </summary>
    public string NetworkName { get; set; } = "Internet Relay Chat";

    /// <summary>
    ///     The client's host mask (nickname!user@host)
    /// </summary>
    public string HostMask { get; set; }

    /// <summary>
    ///     Gets the welcome message text based on properties
    /// </summary>
    public string WelcomeMessage =>
        string.IsNullOrEmpty(HostMask)
            ? $"Welcome to the {NetworkName} Network {Nickname}"
            : $"Welcome to the {NetworkName} Network {HostMask}";

    /// <summary>
    ///     Custom welcome message (overrides the standard format if set)
    /// </summary>
    public string CustomMessage { get; set; }

    public override void Parse(string line)
    {
        // Example: :irc.server.net 001 nickname :Welcome to the Internet Relay Chat Network nickname!user@host
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "001"
        Nickname = parts[2];

        // Extract message (removes the leading ":")
        var message = parts[3].StartsWith(':') ? parts[3].Substring(1) : parts[3];
        CustomMessage = message;

        // Try to parse network name and host mask from the message
        const string welcomePrefix = "Welcome to the ";
        const string networkSuffix = " Network ";

        var welcomeIndex = message.IndexOf(welcomePrefix);
        if (welcomeIndex >= 0)
        {
            var networkStartIndex = welcomeIndex + welcomePrefix.Length;
            var networkEndIndex = message.IndexOf(networkSuffix, networkStartIndex);

            if (networkEndIndex > networkStartIndex)
            {
                NetworkName = message.Substring(networkStartIndex, networkEndIndex - networkStartIndex);

                // The host mask should be everything after the network suffix
                var hostMaskIndex = networkEndIndex + networkSuffix.Length;
                if (hostMaskIndex < message.Length)
                {
                    HostMask = message.Substring(hostMaskIndex);
                }
            }
        }
    }

    public override string Write()
    {
        var message = !string.IsNullOrEmpty(CustomMessage)
            ? CustomMessage
            : WelcomeMessage;

        return $":{ServerName} 001 {Nickname} :{message}";
    }

    /// <summary>
    ///     Creates an RPL_WELCOME reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="networkName">The IRC network name</param>
    /// <param name="hostMask">The client's host mask (nickname!user@host)</param>
    /// <returns>A formatted RPL_WELCOME response</returns>
    public static RplWelcome Create(
        string serverName,
        string nickname,
        string networkName,
        string hostMask = null
    )
    {
        return new RplWelcome
        {
            ServerName = serverName,
            Nickname = nickname,
            NetworkName = networkName,
            HostMask = hostMask ?? nickname
        };
    }

    /// <summary>
    ///     Creates an RPL_WELCOME reply with a custom message
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="customMessage">Custom welcome message text</param>
    /// <returns>A formatted RPL_WELCOME response with custom message</returns>
    public static RplWelcome CreateWithCustomMessage(
        string serverName,
        string nickname,
        string customMessage
    )
    {
        return new RplWelcome
        {
            ServerName = serverName,
            Nickname = nickname,
            CustomMessage = customMessage
        };
    }
}
