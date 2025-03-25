using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// :irc.example.net 002 Mario :Your host is irc.example.net running version ircd-2.11.2
/// </summary>
public class RplYourHostCommand : BaseIrcCommand
{
    public string Host { get; set; }

    public string Username { get; set; }

    public string Message { get; set; }

    public RplYourHostCommand() : base("002")
    {

    }

    public RplYourHostCommand(string host, string username, string message) : base("002")
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
