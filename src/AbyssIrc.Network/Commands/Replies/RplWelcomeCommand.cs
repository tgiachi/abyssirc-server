using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     :irc.example.net 001 Mario :Welcome to the Internet Relay Chat Network Mario
/// </summary>
public class RplWelcomeCommand : BaseIrcCommand
{
    public RplWelcomeCommand() : base("001")
    {
    }

    public RplWelcomeCommand(string host, string username, string? message = null) : base("001")
    {
        Host = host;
        Username = username;

        Message ??= $"Welcome to the {host} Network, {username}";
    }

    public string Host { get; set; }

    public string Username { get; set; }

    public string Message { get; set; }


    public override string Write()
    {
        return $":{Host} {Code} {Username} :{Message}";
    }
}
