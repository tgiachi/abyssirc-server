using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_VERSION (351) numeric reply showing server version information
/// </summary>
public class RplVersion : BaseIrcCommand
{
    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The version string of the server software
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// The server hostname
    /// </summary>
    public string ServerHost { get; set; }

    /// <summary>
    /// Additional comments or details
    /// </summary>
    public string Comments { get; set; }

    public RplVersion() : base("351")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.com 351 nickname AbyssIRC-1.0.0 server.com :Additional comments here
        var parts = line.Split(' ', 5);

        if (parts.Length < 5)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "351"
        Nickname = parts[2];
        Version = parts[3];
        ServerHost = parts[4].Split(' ')[0];

        // Extract comments if present
        int colonPos = line.IndexOf(':', parts[0].Length);
        if (colonPos != -1)
        {
            Comments = line.Substring(colonPos + 1);
        }
    }

    public override string Write()
    {
        if (string.IsNullOrEmpty(Comments))
        {
            return $":{ServerName} 351 {Nickname} {Version} {ServerHost}";
        }
        else
        {
            return $":{ServerName} 351 {Nickname} {Version} {ServerHost} :{Comments}";
        }
    }

    /// <summary>
    /// Creates a RPL_VERSION reply
    /// </summary>
    public static RplVersion Create(
        string serverName, string nickname, string version,
        string serverHost, string comments = null
    )
    {
        return new RplVersion
        {
            ServerName = serverName,
            Nickname = nickname,
            Version = version,
            ServerHost = serverHost,
            Comments = comments
        };
    }
}
