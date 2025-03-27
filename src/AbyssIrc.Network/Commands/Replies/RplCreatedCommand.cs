using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     :irc.example.net 003 Mario :This server was created Mon Jan 15 2024 at 10:15:00 UTC
/// </summary>
public class RplCreatedCommand : BaseIrcCommand
{
    public RplCreatedCommand() : base("003")
    {
    }

    public RplCreatedCommand(string host, string username, string? message = null) : base("003")
    {
        Host = host;
        Username = username;
        Message = message ??
                  $"This server was created {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";
    }

    public string Host { get; set; }

    public string Username { get; set; }

    public string Message { get; set; }

    public override string Write()
    {
        return $":{Host} {Code} {Username} :{Message}";
    }
}
