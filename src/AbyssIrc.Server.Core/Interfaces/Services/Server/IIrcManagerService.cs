using AbyssIrc.Core.Interfaces.Services;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Interfaces.Listener;

namespace AbyssIrc.Server.Core.Interfaces.Services.Server;

public interface IIrcManagerService : IAbyssStarStopService
{
    Task DispatchMessageAsync(string id, string command);

    void RegisterListener(IIrcCommand command, IIrcMessageListener listener);

    void RegisterListener(string commandCode, Func<string, IIrcCommand, Task> callback);

    Task StartAsync();

    Task SendNoticeMessageAsync(string sessionId, string target, string message);
}
