using AbyssIrc.Network.Interfaces.Commands;

namespace AbyssIrc.Server.Data.Events.Irc;

public record SendIrcMessageEvent(string Id, IIrcCommand Message);
