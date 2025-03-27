using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents the RPL_ISUPPORT (005) command that informs clients about supported features
/// </summary>
public class RplISupport : BaseIrcCommand
{
    public RplISupport() : base("005")
    {
    }

    /// <summary>
    ///     Nickname of the client to whom the message is addressed
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     Name of the server
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     List of support tokens
    /// </summary>
    public List<string> SupportTokens { get; set; } = new();

    public override void Parse(string line)
    {
        // Example: :atw.hu.quakenet.org 005 nickname token1 token2 token3 :are supported by this server
        var parts = line.Split(' ');

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "005"
        Nickname = parts[2];

        // Find the index where the final part ":are supported..." begins
        var lastPartIndex = -1;
        for (var i = 3; i < parts.Length; i++)
        {
            if (parts[i].StartsWith(":are") || parts[i].StartsWith(":is") || parts[i].StartsWith(":supports"))
            {
                lastPartIndex = i;
                break;
            }
        }

        // If there's no final part, take all tokens
        var endIndex = lastPartIndex > 0 ? lastPartIndex : parts.Length;

        // Extract support tokens
        for (var i = 3; i < endIndex; i++)
        {
            SupportTokens.Add(parts[i]);
        }
    }

    public override string Write()
    {
        // Format a list of tokens into a string
        var tokens = string.Join(" ", SupportTokens);
        return $":{ServerName} 005 {Nickname} {tokens} :are supported by this server";
    }

    /// <summary>
    ///     Creates an RPL_ISUPPORT message with the specified parameters
    /// </summary>
    public static RplISupport Create(string serverName, string nickname, params string[] tokens)
    {
        return new RplISupport
        {
            ServerName = serverName,
            Nickname = nickname,
            SupportTokens = tokens.ToList()
        };
    }
}
