using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// :irc.example.net 001 Mario :Welcome to the Internet Relay Chat Network Mario
/// </summary>
public class RplWelcomeCommand : BaseIrcCommand
{
    public string Host { get; set; }

    public string Username { get; set; }

    public string Message { get; set; }


    public RplWelcomeCommand() : base("001")
    {

    }

    public RplWelcomeCommand(string host, string username, string message) : base("001")
    {
        Host = host;
        Username = username;
        Message = message;
    }


    public override string Write()
    {
        return $":{Host} {Code} {Username} :{Message}";
    }
}
