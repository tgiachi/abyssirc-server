using AbyssIrc.Network.Interfaces.Commands;

namespace AbyssIrc.Server.Data.Events;

public record SendIrcMessageEvent(string Id, IIrcCommand Message);
