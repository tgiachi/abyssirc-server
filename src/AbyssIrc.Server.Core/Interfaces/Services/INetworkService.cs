using AbyssIrc.Core.Interfaces.Services;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Network;
using AbyssIrc.Server.Core.Interfaces.Listeners;

namespace AbyssIrc.Server.Core.Interfaces.Services;

public interface INetworkService : IAbyssStarStopService
{
    void RegisterCommand<TCommand>() where TCommand : IIrcCommand, new();

    void RegisterCommandListener<TCommand, TListener>()
        where TCommand : IIrcCommand, new() where TListener : IIrcCommandListener;

    NetworkSessionData? GetSessionById(string sessionId);
}
