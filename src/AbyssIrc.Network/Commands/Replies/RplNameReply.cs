using AbyssIrc.Network.Commands.Base;
using AbyssIrc.Network.Types;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_NAMREPLY (353) numeric reply
/// Lists clients joined to a channel and their status
/// </summary>
public class RplNameReply : BaseIrcCommand
{
    /// <summary>
    /// The server name sending the reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The nickname of the client receiving the reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// Channel visibility status
    /// </summary>
    public ChannelVisibility Visibility { get; set; } = ChannelVisibility.Public;

    /// <summary>
    /// The name of the channel
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    /// List of channel members with their prefixes
    /// </summary>
    public List<string> Members { get; set; } = new List<string>();

    public RplNameReply() : base("353")
    {
    }

    public override void Parse(string line)
    {
        // Reset existing data
        ServerName = null;
        Nickname = null;
        ChannelName = null;
        Members.Clear();
        Visibility = ChannelVisibility.Public;

        // Ensure line starts with server prefix
        if (!line.StartsWith(':'))
        {
            return; // Invalid format
        }

        var parts = line.Split(' ');

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        // Parse server name
        ServerName = parts[0].TrimStart(':');

        // Verify this is a 353 numeric
        if (parts[1] != "353")
        {
            return;
        }

        // Parse nickname
        Nickname = parts[2];

        // Parse channel visibility
        switch (parts[3][0])
        {
            case '=':
                Visibility = ChannelVisibility.Public;
                break;
            case '@':
                Visibility = ChannelVisibility.Secret;
                break;
            case '*':
                Visibility = ChannelVisibility.Private;
                break;
            default:
                Visibility = ChannelVisibility.Public;
                break;
        }

        // Parse channel name (the actual channel name, not the visibility symbol)
        ChannelName = parts[4].StartsWith(':')
            ? parts[4].Substring(1)
            : parts[4];

        // Parse members list
        // Start from index 5, remove leading : if present
        string membersListStr = parts.Length > 5 && parts[5].StartsWith(':')
            ? parts[5].Substring(1)
            : (parts.Length > 5 ? parts[5] : "");

        // Additional parts might contain more of the members list
        for (int i = 6; i < parts.Length; i++)
        {
            membersListStr += " " + parts[i];
        }

        // Split and add members
        Members.AddRange(
            membersListStr.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        );
    }

    public override string Write()
    {
        // Ensure required fields are set
        if (string.IsNullOrEmpty(ServerName) ||
            string.IsNullOrEmpty(Nickname) ||
            string.IsNullOrEmpty(ChannelName) ||
            Members.Count == 0)
        {
            throw new InvalidOperationException("All fields must be set before writing");
        }

        // Construct the NAMES reply
        return $":{ServerName} 353 {Nickname} {(char)Visibility} {ChannelName} :{string.Join(" ", Members)}";
    }

    /// <summary>
    /// Creates a RPL_NAMREPLY for a specific channel
    /// </summary>
    /// <param name="serverName">Server sending the reply</param>
    /// <param name="nickname">Nickname receiving the reply</param>
    /// <param name="channelName">Channel name</param>
    /// <param name="members">List of channel members</param>
    /// <param name="visibility">Channel visibility (default is Public)</param>
    public static RplNameReply Create(
        string serverName,
        string nickname,
        string channelName,
        IEnumerable<string> members,
        ChannelVisibility visibility = ChannelVisibility.Public
    )
    {
        return new RplNameReply
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            Members = new List<string>(members),
            Visibility = visibility
        };
    }
}
