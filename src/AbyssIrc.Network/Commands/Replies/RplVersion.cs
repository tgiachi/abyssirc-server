using System.Reflection;
using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents the RPL_VERSION (351) numeric reply
/// Provides detailed server version information
/// </summary>
public class RplVersion : BaseIrcCommand
{
    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// Server version string
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Server hostname
    /// </summary>
    public string ServerHost { get; set; }

    /// <summary>
    /// Additional comments or details about the server
    /// </summary>
    public string Comments { get; set; }

    public RplVersion() : base("351")
    {
    }

    /// <summary>
    /// Parses the RPL_VERSION numeric reply
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 351 nickname AbyssIRC-1.0.0 server.com :Additional comments here

        // Reset existing data
        ServerName = null;
        Nickname = null;
        Version = null;
        ServerHost = null;
        Comments = null;

        // Check for source prefix
        if (line.StartsWith(':'))
        {
            int spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                ServerName = line.Substring(1, spaceIndex - 1);
                line = line.Substring(spaceIndex + 1).TrimStart();
            }
        }

        // Split remaining parts
        string[] parts = line.Split(' ');

        // Ensure we have enough parts
        if (parts.Length < 4)
            return;

        // Verify the numeric code
        if (parts[0] != "351")
            return;

        // Extract nickname
        Nickname = parts[1];

        // Extract version
        Version = parts[2];

        // Extract server host
        ServerHost = parts[3];

        // Extract comments if present
        int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + parts[2].Length + parts[3].Length + 4);
        if (colonIndex != -1)
        {
            Comments = line.Substring(colonIndex + 1);
        }
    }

    /// <summary>
    /// Converts the reply to its string representation
    /// </summary>
    /// <returns>Formatted RPL_VERSION message</returns>
    public override string Write()
    {
        // Prepare version information
        if (string.IsNullOrEmpty(Version))
        {
            // Use assembly version if not explicitly set
            Version = GetServerVersion();
        }

        // Prepare server host
        if (string.IsNullOrEmpty(ServerHost))
        {
            ServerHost = System.Net.Dns.GetHostName();
        }

        // Format the response
        return string.IsNullOrEmpty(ServerName)
            ? (string.IsNullOrEmpty(Comments)
                ? $"351 {Nickname} {Version} {ServerHost}"
                : $"351 {Nickname} {Version} {ServerHost} :{Comments}")
            : (string.IsNullOrEmpty(Comments)
                ? $":{ServerName} 351 {Nickname} {Version} {ServerHost}"
                : $":{ServerName} 351 {Nickname} {Version} {ServerHost} :{Comments}");
    }

    /// <summary>
    /// Creates a RPL_VERSION reply
    /// </summary>
    /// <param name="serverName">Server sending the reply</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="version">Optional server version</param>
    /// <param name="serverHost">Optional server hostname</param>
    /// <param name="comments">Optional additional comments</param>
    public static RplVersion Create(
        string serverName,
        string nickname,
        string version = null,
        string serverHost = null,
        string comments = null
    )
    {
        return new RplVersion
        {
            ServerName = serverName,
            Nickname = nickname,
            Version = version ?? GetServerVersion(),
            ServerHost = serverHost ?? System.Net.Dns.GetHostName(),
            Comments = comments
        };
    }

    /// <summary>
    /// Retrieves the current server version from the assembly
    /// </summary>
    /// <returns>Server version as a string</returns>
    private static string GetServerVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return $"AbyssIRC-{version}";
    }
}
