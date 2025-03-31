using AbyssIrc.Network.Commands.Base;
using AbyssIrc.Network.Types;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_NAMREPLY (353) numeric reply that lists users in a channel
/// Format: ":server 353 nickname = #channel :@nick1 +nick2 nick3"
/// </summary>
public class RplNamReply : BaseIrcCommand
{
    public RplNamReply() : base("353")
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
    /// The channel name
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// The channel visibility type
    /// </summary>
    public ChannelVisibility VisibilityType { get; set; } = ChannelVisibility.Public;

    /// <summary>
    /// List of nicknames in the channel, with their prefix symbols
    /// </summary>
    public List<string> Nicknames { get; set; } = new List<string>();

    public override void Parse(string line)
    {
        // Example: :irc.server.net 353 MyNick = #channel :@admin +voice regular
        var parts = line.Split(' ');

        if (parts.Length < 6)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "353"
        Nickname = parts[2];

        // Parse visibility type
        switch (parts[3])
        {
            case "=":
                VisibilityType = ChannelVisibility.Public;
                break;
            case "@":
                VisibilityType = ChannelVisibility.Secret;
                break;
            case "*":
                VisibilityType = ChannelVisibility.Private;
                break;
        }

        ChannelName = parts[4];

        // Find names list by locating the colon
        var colonPos = line.IndexOf(':', parts[0].Length);
        if (colonPos != -1)
        {
            var namesList = line.Substring(colonPos + 1);
            Nicknames = namesList.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }

    public override string Write()
    {
        // Convert visibility type to symbol
        string visibilitySymbol = VisibilityType switch
        {
            ChannelVisibility.Public  => "=",
            ChannelVisibility.Secret  => "@",
            ChannelVisibility.Private => "*",
            _                         => "="
        };

        // Combine nicknames with spaces
        var nicknames = string.Join(" ", Nicknames);

        return $":{ServerName} 353 {Nickname} {visibilitySymbol} {ChannelName} :{nicknames}";
    }

    /// <summary>
    /// Creates an RPL_NAMREPLY response
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="channelName">The channel name</param>
    /// <param name="nicknames">List of nicknames in the channel (with their prefix symbols)</param>
    /// <param name="visibilityType">Channel visibility type</param>
    /// <returns>A formatted RPL_NAMREPLY response</returns>
    public static RplNamReply Create(
        string serverName,
        string nickname,
        string channelName,
        IEnumerable<string> nicknames,
        ChannelVisibility visibilityType = ChannelVisibility.Public
    )
    {
        return new RplNamReply
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            Nicknames = nicknames.ToList(),
            VisibilityType = visibilityType
        };
    }

    /// <summary>
    /// Splits a list of nicknames into multiple RPL_NAMREPLY messages if necessary
    /// to respect IRC message length limits
    /// </summary>
    /// <param name="serverName">The server name</param>
    /// <param name="nickname">The target nickname</param>
    /// <param name="channelName">The channel name</param>
    /// <param name="nicknames">List of nicknames in the channel (with their prefix symbols)</param>
    /// <param name="visibilityType">Channel visibility type</param>
    /// <param name="maxLength">Maximum length of each names list (default 400)</param>
    /// <returns>List of RPL_NAMREPLY messages</returns>
    public static List<RplNamReply> CreateSplit(
        string serverName,
        string nickname,
        string channelName,
        IEnumerable<string> nicknames,
        ChannelVisibility visibilityType = ChannelVisibility.Public,
        int maxLength = 400
    )
    {
        var result = new List<RplNamReply>();
        var nicknamesList = nicknames.ToList();

        if (!nicknamesList.Any())
        {
            // If no nicknames, return a single empty reply
            result.Add(Create(serverName, nickname, channelName, new List<string>(), visibilityType));
            return result;
        }

        var currentNames = new List<string>();
        var currentLength = 0;

        foreach (var nick in nicknamesList)
        {
            // The +1 is for the space between names
            if (currentLength + nick.Length + 1 > maxLength && currentNames.Count > 0)
            {
                // Current batch would exceed length limit, create a new message
                result.Add(Create(serverName, nickname, channelName, currentNames, visibilityType));
                currentNames = new List<string>();
                currentLength = 0;
            }

            currentNames.Add(nick);
            currentLength += nick.Length + 1; // +1 for the space
        }

        // Add the final batch
        if (currentNames.Count > 0)
        {
            result.Add(Create(serverName, nickname, channelName, currentNames, visibilityType));
        }

        return result;
    }
}
