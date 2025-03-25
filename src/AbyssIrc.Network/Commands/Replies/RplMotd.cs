using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents the RPL_MOTD (372) numeric reply for a line of the MOTD
/// </summary>
public class RplMotd : BaseIrcCommand
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
    /// A line of text from the MOTD
    /// </summary>
    public string Text { get; set; }

    public RplMotd() : base("372")
    {
    }


    public RplMotd(string serverName, string nickname, string text) : base("372")
    {
        ServerName = serverName;
        Nickname = nickname;
        Text = text;
    }

    public override void Parse(string line)
    {
        // Example: :server.com 372 nickname :- Welcome to IRC!
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "372"
        Nickname = parts[2];
        Text = parts[3].TrimStart(':');
    }

    public override string Write()
    {
        return $":{ServerName} 372 {Nickname} :- {Text}";
    }

    /// <summary>
    /// Creates a RPL_MOTD reply
    /// </summary>
    public static RplMotd Create(string serverName, string nickname, string text)
    {
        return new RplMotd
        {
            ServerName = serverName,
            Nickname = nickname,
            Text = text
        };
    }
}
