using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents RPL_CHANNELMODEIS (324) numeric reply that shows the current channel modes
/// </summary>
public class RplChannelModeIs : BaseIrcCommand
{
    public RplChannelModeIs() : base("324")
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
    ///     The channel name
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The mode string including the plus sign (e.g., "+nt")
    /// </summary>
    public string ModeString { get; set; }

    /// <summary>
    ///     Additional parameters for modes that require them
    /// </summary>
    public List<string> ModeParameters { get; set; } = new();

    public override void Parse(string line)
    {
        // Example: :server.com 324 nickname #channel +nt key
        var parts = line.Split(' ');

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "324"
        Nickname = parts[2];
        ChannelName = parts[3];
        ModeString = parts[4];

        // Collect any parameters
        for (var i = 5; i < parts.Length; i++)
        {
            ModeParameters.Add(parts[i]);
        }
    }

    public override string Write()
    {
        var result = $":{ServerName} 324 {Nickname} {ChannelName} {ModeString}";

        if (ModeParameters.Count > 0)
        {
            result += " " + string.Join(" ", ModeParameters);
        }

        return result;
    }

    /// <summary>
    ///     Creates a RPL_CHANNELMODEIS reply
    /// </summary>
    public static RplChannelModeIs Create(
        string serverName, string nickname, string channelName,
        string modeString, params string[] parameters
    )
    {
        return new RplChannelModeIs
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            ModeString = modeString,
            ModeParameters = parameters?.ToList() ?? new List<string>()
        };
    }
}
