using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
/// Represents RPL_YOURHOST (002) numeric reply
/// Sent as part of the initial welcome sequence to inform the client about the server
/// Format: ":server 002 nickname :Your host is server.name, running version X.Y.Z"
/// </summary>
public class RplYourHost : BaseIrcCommand
{
    public RplYourHost() : base("002")
    {
    }

    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The server software version
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Additional information about the server (optional)
    /// </summary>
    public string AdditionalInfo { get; set; } = string.Empty;

    public override void Parse(string line)
    {
        // Example: :irc.server.net 002 nickname :Your host is irc.server.net, running version AbyssIRC-1.0.0
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "002"
        Nickname = parts[2];

        // Extract message (removes the leading ":")
        string message = parts[3].StartsWith(':') ? parts[3].Substring(1) : parts[3];

        // Try to parse out version from the message
        const string hostMarker = "Your host is ";
        const string versionMarker = "running version ";

        int hostIndex = message.IndexOf(hostMarker);
        int versionIndex = message.IndexOf(versionMarker);

        if (versionIndex > 0)
        {
            Version = message.Substring(versionIndex + versionMarker.Length).Trim();

            // Try to extract additional info if present
            int additionalInfoIndex = Version.IndexOf(',');
            if (additionalInfoIndex > 0)
            {
                AdditionalInfo = Version.Substring(additionalInfoIndex + 1).Trim();
                Version = Version.Substring(0, additionalInfoIndex).Trim();
            }
        }
    }

    public string Message => $"Your host is {ServerName}, running version {Version}";

    public override string Write()
    {
        string versionInfo = !string.IsNullOrEmpty(AdditionalInfo)
            ? $"{Version}, {AdditionalInfo}"
            : Version;

        return $":{ServerName} 002 {Nickname} :Your host is {ServerName}, running version {versionInfo}";
    }

    /// <summary>
    /// Creates an RPL_YOURHOST reply
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="version">The server software version</param>
    /// <param name="additionalInfo">Optional additional information</param>
    /// <returns>A formatted RPL_YOURHOST response</returns>
    public static RplYourHost Create(
        string serverName,
        string nickname,
        string version,
        string additionalInfo = "")
    {
        return new RplYourHost
        {
            ServerName = serverName,
            Nickname = nickname,
            Version = version,
            AdditionalInfo = additionalInfo
        };
    }
}
