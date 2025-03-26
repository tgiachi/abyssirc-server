using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
/// Represents ERR_NORECIPIENT (411) numeric reply
/// </summary>
public class ErrNoRecipient : BaseIrcCommand
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
    /// The command that caused the error
    /// </summary>
    public string Command { get; set; } = "PRIVMSG";

    public ErrNoRecipient() : base("411")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.com 411 nickname :No recipient given (PRIVMSG)
        var parts = line.Split(' ', 3);

        if (parts.Length < 3)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "411"
        Nickname = parts[2].Split(' ')[0];

        // Try to extract command from error message
        string errorText = parts[2].Substring(parts[2].IndexOf(':') + 1);
        int openBracket = errorText.IndexOf('(');
        int closeBracket = errorText.IndexOf(')');

        if (openBracket != -1 && closeBracket != -1 && closeBracket > openBracket)
        {
            Command = errorText.Substring(openBracket + 1, closeBracket - openBracket - 1);
        }
    }

    public override string Write()
    {
        return $":{ServerName} {Code} {Nickname} :No recipient given ({Command})";
    }

    /// <summary>
    /// Creates an ERR_NORECIPIENT reply
    /// </summary>
    public static ErrNoRecipient Create(string serverName, string nickname, string command = "PRIVMSG")
    {
        return new ErrNoRecipient
        {
            ServerName = serverName,
            Nickname = nickname,
            Command = command
        };
    }
}
