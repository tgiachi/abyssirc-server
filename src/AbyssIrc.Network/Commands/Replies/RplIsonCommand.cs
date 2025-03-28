using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents an IRC RPL_ISON (303) response
/// Returned in response to the ISON command with a list of nicknames that are currently online
/// </summary>
public class RplIsonCommand : BaseIrcCommand
{
    /// <summary>
    /// The server name/source of the response
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// List of nicknames that are currently online
    /// </summary>
    public List<string> OnlineNicknames { get; set; } = new List<string>();

    public RplIsonCommand() : base("303")
    {
    }

    public override void Parse(string line)
    {
        // RPL_ISON format: ":server 303 nickname :nick1 nick2 ..."

        if (!line.StartsWith(':'))
        {
            return; // Invalid format
        }

        var parts = line.Split(' ', 4); // Maximum of 4 parts

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "303"
        Nickname = parts[2];

        // Extract the online nicknames (removes the leading ":")
        string nicknames = parts[3].StartsWith(':') ? parts[3].Substring(1) : parts[3];

        // Split the nicknames and add them to the list
        foreach (var nick in nicknames.Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
        {
            OnlineNicknames.Add(nick);
        }
    }

    public override string Write()
    {
        // Format: ":server 303 nickname :nick1 nick2 ..."
        string nicknames = string.Join(" ", OnlineNicknames);
        return $":{ServerName} 303 {Nickname} :{nicknames}";
    }
}
