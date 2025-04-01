using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Errors;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Server.Data.Internal.ServiceCollection;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services.Server;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;


namespace AbyssIrc.Server.Services;

public class IrcManagerService : IIrcManagerService
{
    private const string _processQueueContext = "irc_manager";
    private readonly IAbyssSignalService _signalService;
    private readonly ILogger _logger;

    private readonly Dictionary<string, List<IIrcMessageListener>> _listeners = new();

    private readonly List<IrcHandlerDefinitionData> _ircHandlers;

    private readonly IIrcCommandParser _commandParser;

    private readonly IServiceProvider _serviceProvider;

    private readonly AbyssIrcConfig _abyssIrcConfig;

    private readonly IProcessQueueService _processQueueService;

    public IrcManagerService(
        ILogger<IrcManagerService> logger, IAbyssSignalService signalService, List<IrcHandlerDefinitionData> ircHandlers,
        IServiceProvider serviceProvider, IIrcCommandParser commandParser, AbyssIrcConfig abyssIrcConfig,
        IProcessQueueService processQueueService
    )
    {
        _signalService = signalService;
        _ircHandlers = ircHandlers;
        _serviceProvider = serviceProvider;
        _commandParser = commandParser;
        _abyssIrcConfig = abyssIrcConfig;
        _processQueueService = processQueueService;
        _logger = logger;
    }

    public async Task DispatchMessageAsync(string id, string command)
    {
        // await _processQueueService.Enqueue(
        //     _processQueueContext,
        //     async () => { await ParseMessageAsync(id, command); }
        // );
        await ParseMessageAsync(id, command);
    }

    private async Task ParseMessageAsync(string id, string command)
    {
        _logger.LogDebug(
            "Parsing message '{Message}' from session '{SessionId}' and ThreadId: {ThreadId}",
            command,
            id,
            Environment.CurrentManagedThreadId
        );
        var commands = await _commandParser.ParseAsync(command);

        foreach (var cmd in commands)
        {
            await _signalService.PublishAsync(new IrcMessageReceivedEvent(id, cmd));

            if (_listeners.TryGetValue(cmd.Code, out var listeners))
            {
                if (cmd is not NotParsedCommand)
                {
                    foreach (var listener in listeners)
                    {
                        try
                        {
                            await listener.OnMessageReceivedAsync(id, cmd);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(
                                e,
                                "Error while executing listener '{Listener}' for command '{Command}'",
                                listener.GetType().Name,
                                cmd.Code
                            );
                        }
                    }
                }
                else
                {
                    await _signalService.PublishAsync(new IrcUnknownCommandEvent(id, (NotParsedCommand)cmd));
                }
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

    public void RegisterListener(string commandCode, Func<string, IIrcCommand, Task> callback)
    {
        if (string.IsNullOrEmpty(commandCode))
        {
            throw new ArgumentNullException(nameof(commandCode), "Command code cannot be null or empty");
        }

        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback), "Callback function cannot be null");
        }

        if (!_listeners.ContainsKey(commandCode))
        {
            _listeners.Add(commandCode, new List<IIrcMessageListener>());
        }

        // Create a wrapper that implements IIrcMessageListener
        var callbackWrapper = new IrcCallbackListener(callback);

        _logger.LogDebug(
            "Registering callback listener for command code '{Code}'",
            commandCode
        );

        _listeners[commandCode].Add(callbackWrapper);
    }

    public Task StartAsync()
    {
        foreach (var ircHandler in _ircHandlers)
        {
            _logger.LogDebug("Starting handler '{Handler}'", ircHandler.HandlerType.Name);
            _serviceProvider.GetService(ircHandler.HandlerType);
        }

        return Task.CompletedTask;
    }

    public async Task SendNoticeMessageAsync(string sessionId, string target, string message)
    {
        var noticeCommand = new NoticeCommand()
        {
            Source = _abyssIrcConfig.Network.Host,
            Message = message,
            Target = target
        };


        var sendIrcMessageEvent = new SendIrcMessageEvent(sessionId, noticeCommand);

        await _signalService.PublishAsync(sendIrcMessageEvent);
    }


    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}
