using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
/// Represents ERR_NOTEXTTOSEND (412) numeric reply
/// </summary>
public class ErrNoTextToSend : BaseIrcCommand
{
    /// <summary>
    /// The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    public ErrNoTextToSend() : base("412")
    {
    }

    public override void Parse(string line)
    {
        // Example: :server.com 412 nickname :No text to send
        var parts = line.Split(' ', 3);

        if (parts.Length < 3)
            return; // Invalid format

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "412"
        Nickname = parts[2].Split(' ')[0];
    }

    public override string Write()
    {
        return $":{ServerName} {Code} {Nickname} :No text to send";
    }

    /// <summary>
    /// Creates an ERR_NOTEXTTOSEND reply
    /// </summary>
    public static ErrNoTextToSend Create(string serverName, string nickname)
    {
        return new ErrNoTextToSend
        {
            ServerName = serverName,
            Nickname = nickname
        };
    }
}

