using AbyssIrc.Server.Core.Data.Sessions;

namespace AbyssIrc.Server.Data.Events.Sessions;

public record SessionRemovedEvent(string Id, IrcSession Session);
