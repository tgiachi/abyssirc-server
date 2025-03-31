using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents the RPL_MYINFO (004) numeric reply that provides server information to the client
/// This command gives details about the server's capabilities, version, and supported modes
/// Part of the initial server registration process
/// </summary>
public class RplMyInfo : BaseIrcCommand
{
    /// <summary>
    /// Name of the IRC server sending the information
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// Version of the IRC server software
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// User modes supported by the server
    /// </summary>
    public string UserModes { get; set; }

    /// <summary>
    /// Channel modes supported by the server
    /// </summary>
    public string ChannelModes { get; set; }

    /// <summary>
    /// Nickname of the client receiving this message
    /// </summary>
    public string TargetNick { get; set; }

    /// <summary>
    /// Initializes a new instance of the RPL_MYINFO command
    /// </summary>
    public RplMyInfo() : base("004")
    {
    }

    /// <summary>
    /// Creates a RPL_MYINFO command with server configuration details
    /// </summary>
    /// <param name="serverConfig">Server configuration containing mode and network details</param>
    /// <param name="nickname">Nickname of the client receiving the information</param>
    /// <returns>Configured RPL_MYINFO command</returns>
    public static RplMyInfo Create(string hostname, string userModes, string channelModes, string nickname)
    {
        return new RplMyInfo
        {
            ServerName = hostname,
            Version = GetServerVersion(),
            UserModes = userModes,
            ChannelModes = channelModes,
            TargetNick = nickname
        };
    }

    /// <summary>
    /// Retrieves the current server version from the assembly
    /// </summary>
    /// <returns>Server version as a string</returns>
    private static string GetServerVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        return assembly.GetName().Version.ToString();
    }

    /// <summary>
    /// Generates the string representation of the RPL_MYINFO command
    /// </summary>
    /// <returns>Formatted RPL_MYINFO message</returns>
    public override string Write()
    {
        return $":{ServerName} 004 {TargetNick} {ServerName} {Version} {UserModes} {ChannelModes}";
    }

    /// <summary>
    /// Parses an incoming RPL_MYINFO message
    /// </summary>
    /// <param name="rawMessage">Raw IRC message to parse</param>
    public override void Parse(string rawMessage)
    {
        // Example: :irc.example.net 004 Mario irc.example.net ircd-2.11.2 aoOirw biklmnopstv
        var parts = rawMessage.Split(' ');

        if (parts.Length >= 6)
        {
            ServerName = parts[0].TrimStart(':');
            TargetNick = parts[2];
            Version = parts[4];
            UserModes = parts[5];

            // Channel modes might be in the last element
            if (parts.Length >= 7)
            {
                ChannelModes = parts[6];
            }
        }
    }
}
