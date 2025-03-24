using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

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

    public override string Write()
    {
        return $":{Host} {Code} {Username} :{Message}";
    }
}
