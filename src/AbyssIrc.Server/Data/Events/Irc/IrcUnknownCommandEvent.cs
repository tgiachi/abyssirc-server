using AbyssIrc.Network.Commands;

namespace AbyssIrc.Server.Data.Events.Irc;

public record IrcUnknownCommandEvent(string Id, NotParsedCommand Command);
