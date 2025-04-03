using AbyssIrc.Protocol.Messages.Interfaces.Commands;

namespace AbyssIrc.Server.Data.Internal.ServiceCollection;

public record IrcCommandListenerDefinitionData(Type HandlerType, IIrcCommand Command);
