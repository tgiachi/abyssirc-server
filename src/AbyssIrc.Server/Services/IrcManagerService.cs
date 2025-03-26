using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Interfaces.Services.Server;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;


namespace AbyssIrc.Server.Services;

public class IrcManagerService : IIrcManagerService
{
    private readonly IAbyssSignalService _signalService;
    private readonly ILogger _logger;

    private readonly Dictionary<string, List<IIrcMessageListener>> _listeners = new();


    public IrcManagerService(ILogger<IrcManagerService> logger, IAbyssSignalService signalService)
    {
        _signalService = signalService;
        _logger = logger;
    }

    public async Task DispatchMessageAsync(string id, IIrcCommand command)
    {
        await _signalService.PublishAsync(new IrcMessageReceivedEvent(id, command));


        if (_listeners.TryGetValue(command.Code, out var listeners))
        {
            foreach (var listener in listeners)
            {
                await listener.OnMessageReceivedAsync(id, command);
            }
        }
    }

    public void RegisterListener(IIrcCommand command, IIrcMessageListener listener)
    {
        if (!_listeners.ContainsKey(command.Code))
        {
            _listeners.Add(command.Code, []);
        }

        _logger.LogDebug(
            "Registering listener for command '{Command}'({Code}) with listener '{Listener}'",
            command.GetType().Name,
            command.Code,
            listener.GetType().Name
        );

        _listeners[command.Code].Add(listener);
    }
}
