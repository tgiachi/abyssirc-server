using AbyssIrc.Network.Interfaces.Commands;

namespace AbyssIrc.Server.Data.Events;

public record IrcMessageReceivedEvent(string Id, IIrcCommand Command);
