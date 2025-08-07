using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents an IRC NAMES command used to request a list of users in a channel
///     Format: "NAMES [#channel[,#channel2...]] [target_server]"
/// </summary>
public class NamesCommand : BaseIrcCommand
{
    public NamesCommand() : base("NAMES")
    {
    }

    /// <summary>
    ///     Source of the command (typically empty for client-originated commands)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     List of channels to query
    ///     If empty, the server should list all channels and users
    /// </summary>
    public List<string> Channels { get; set; } = new();

    /// <summary>
    ///     Optional target server for the query (rarely used in modern IRC)
    /// </summary>
    public string TargetServer { get; set; }

    public override void Parse(string line)
    {
        // Examples:
        // NAMES #channel
        // NAMES #channel1,#channel2
        // NAMES #channel irc.server.net
        // NAMES
        // :nick!user@host NAMES #channel

        // Parse source/prefix if present
        if (line.StartsWith(':'))
        {
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex == -1)
            {
                return; // Invalid format
            }

            Source = line.Substring(1, spaceIndex - 1);
            line = line.Substring(spaceIndex + 1).TrimStart();
        }

        // Split the message into tokens
        var tokens = line.Split(' ');

        // First token should be "NAMES"
        if (tokens.Length < 1 || tokens[0].ToUpper() != "NAMES")
        {
            return;
        }

        // Parse channel list if provided
        if (tokens.Length > 1 && !string.IsNullOrEmpty(tokens[1]) && tokens[1] != ":")
        {
            var channelList = tokens[1];
            Channels = channelList.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        // Parse target server if provided
        if (tokens.Length > 2 && !tokens[2].StartsWith(':'))
        {
            TargetServer = tokens[2];
        }
    }

    public override string Write()
    {
        var result = !string.IsNullOrEmpty(Source) ? $":{Source} NAMES" : "NAMES";

        if (Channels.Count > 0)
        {
            result += " " + string.Join(",", Channels);
        }

        if (!string.IsNullOrEmpty(TargetServer))
        {
            result += " " + TargetServer;
        }

        return result;
    }

    /// <summary>
    ///     Creates a NAMES command for a specific channel
    /// </summary>
    /// <param name="channel">The channel name</param>
    /// <returns>A formatted NAMES command</returns>
    public static NamesCommand Create(string channel)
    {
        return new NamesCommand
        {
            Channels = new List<string> { channel }
        };
    }

    /// <summary>
    ///     Creates a NAMES command for multiple channels
    /// </summary>
    /// <param name="channels">List of channel names</param>
    /// <returns>A formatted NAMES command</returns>
    public static NamesCommand Create(IEnumerable<string> channels)
    {
        return new NamesCommand
        {
            Channels = channels.ToList()
        };
    }

    /// <summary>
    ///     Creates a NAMES command for all channels
    /// </summary>
    /// <returns>A formatted NAMES command</returns>
    public static NamesCommand CreateGlobal()
    {
        return new NamesCommand();
    }
}
