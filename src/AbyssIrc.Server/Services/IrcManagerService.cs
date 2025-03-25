using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;


namespace AbyssIrc.Server.Services;

public class IrcManagerService : IIrcManagerService
{
    private readonly IAbyssIrcSignalEmitterService _signalEmitterService;
    private readonly ILogger _logger;

    private readonly Dictionary<string, List<IIrcMessageListener>> _listeners = new();


    public IrcManagerService(ILogger<IrcManagerService> logger, IAbyssIrcSignalEmitterService signalEmitterService)
    {
        _signalEmitterService = signalEmitterService;
        _logger = logger;
    }

    public async Task DispatchMessageAsync(string id, IIrcCommand command)
    {
        await _signalEmitterService.PublishAsync(new IrcMessageReceivedEvent(id, command));


        if (_listeners.TryGetValue(command.Code, out var listeners))
        {
            foreach (var listener in listeners)
            {
                var message = await listener.OnMessageReceivedAsync(id, command);

                if (message != null)
                {
                    await _signalEmitterService.PublishAsync(new SendIrcMessageEvent(id, message));
                }
            }
        }
    }

    public void RegisterListener(string command, IIrcMessageListener listener)
    {
        if (!_listeners.ContainsKey(command))
        {
            _listeners.Add(command, []);
        }

        _logger.LogDebug(
            "Registering listener for command '{Command}' with listener '{Listener}'",
            command,
            listener.GetType().Name
        );

        _listeners[command].Add(listener);
    }
}
