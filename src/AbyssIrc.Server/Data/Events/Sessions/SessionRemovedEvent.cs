using AbyssIrc.Server.Data.Internal.Sessions;

namespace AbyssIrc.Server.Data.Events.Sessions;

public record SessionRemovedEvent(string Id, IrcSession Session);
