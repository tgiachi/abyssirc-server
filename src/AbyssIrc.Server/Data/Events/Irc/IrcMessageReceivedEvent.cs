using AbyssIrc.Network.Interfaces.Commands;

namespace AbyssIrc.Server.Data.Events.Irc;

public record IrcMessageReceivedEvent(string Id, IIrcCommand Command);
