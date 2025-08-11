using System.Collections.Concurrent;
using System.Net;
using AbyssIrc.Core.Utils;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Protocol.Messages.Interfaces.Parser;
using AbyssIrc.Server.Core.Data.Config;
using AbyssIrc.Server.Core.Data.Network;
using AbyssIrc.Server.Core.Interfaces.Listeners;
using AbyssIrc.Server.Core.Interfaces.Services;
using AbyssIrc.Server.Network.Data;
using AbyssIrc.Server.Network.Servers.Tcp;
using DryIoc;
using Microsoft.Extensions.ObjectPool;
using NanoidDotNet;
using Serilog;

namespace AbyssIrc.Server.Services;

public class NetworkService : INetworkService
{
    private readonly AbyssIrcServerConfig _abyssIrcServerConfig;
    private readonly ILogger _logger = Log.ForContext<NetworkService>();

    private readonly IContainer _container;

    private readonly IProcessQueueService _processQueueService;
    private readonly IIrcCommandParser _commandParser;

    private readonly ObjectPool<NetworkSessionData> _sessionPool =
        new DefaultObjectPool<NetworkSessionData>(new DefaultPooledObjectPolicy<NetworkSessionData>());

    private readonly ConcurrentDictionary<string, NetworkSessionData> _sessions = new();
    private readonly ConcurrentDictionary<string, MoongateTcpClient> _clients = new();

    private readonly Dictionary<string, LinkedList<IIrcCommandListener>> _listeners = new();

    private readonly Dictionary<string, ServerDataObject> _servers = new();

    public NetworkService(
        AbyssIrcServerConfig abyssIrcServerConfig, IProcessQueueService processQueueService, IIrcCommandParser commandParser,
        IContainer container
    )
    {
        _abyssIrcServerConfig = abyssIrcServerConfig;
        _processQueueService = processQueueService;
        _commandParser = commandParser;
        _container = container;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _processQueueService.EnsureContext("network");

        foreach (var bind in _abyssIrcServerConfig.Network.Binds)
        {
            if (!bind.UseSsl)
            {
                foreach (var port in bind.Ports)
                {
                    var id = await Nanoid.GenerateAsync();
                    _logger.Debug("Starting non-SSL bind");

                    var moonTcpServer = new MoongateTcpServer(id, new IPEndPoint(bind.Host, port));

                    moonTcpServer.OnClientConnected += (client) => OnClientConnected(id, client);
                    moonTcpServer.OnClientDisconnected += (client) => OnClientDisconnected(id, client);
                    moonTcpServer.OnError += (err) => OnServerError(id, err);

                    moonTcpServer.OnClientDataReceived += (client, data) => OnDataReceived(id, client, data);
                    moonTcpServer.Start();
                }
            }
        }
    }

    private void OnDataReceived(string id, MoongateTcpClient client, ReadOnlyMemory<byte> data)
    {
        _processQueueService.Enqueue("network", () => ProcessData(id, client, data));
    }

    private void OnClientConnected(string id, MoongateTcpClient moonTcpClient)
    {
        _logger.Information("Client connected");
        var session = _sessionPool.Get();

        session.Id = id;
        session.OnSendMessages += OnSendMessages;

        _sessions.TryAdd(id, session);
        _clients.TryAdd(id, moonTcpClient);
    }

    private void OnSendMessages(string id, IIrcCommand[] commands)
    {
        try
        {
            _processQueueService.Enqueue(
                "network",
                async () =>
                {
                    var tcpSession = _clients[id];

                    if (tcpSession == null)
                    {
                        throw new Exception("Client not found");
                    }

                    var messages = new List<string>();

                    foreach (var command in commands)
                    {
                        messages.Add(await _commandParser.SerializeAsync(command));
                    }

                    var span = StringListToBytesConverter.ConvertWithArrayPool(messages);

                    tcpSession.Send(span.ToArray());
                }
            );
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during sending messages: Count: {Count} to Id:{Id}", commands.Length, id);
        }
    }

    private void OnClientDisconnected(string id, MoongateTcpClient moonTcpClient)
    {
        _logger.Information("Client disconnected");
        if (_sessions.TryRemove(id, out var session))
        {
            _sessionPool.Return(session);
        }

        _clients.TryRemove(id, out _);
    }

    private void OnServerError(string id, Exception ex)
    {
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
    }

    public void RegisterCommand<TCommand>() where TCommand : IIrcCommand, new()
    {
        _commandParser.RegisterCommand<TCommand>();
    }

    public void RegisterCommandListener<TCommand, TListener>() where TCommand : IIrcCommand, new()
        where TListener : IIrcCommandListener
    {
        RegisterCommand<TCommand>();

        var cmd = new TCommand();

        if (!_listeners.TryGetValue(cmd.Code, out LinkedList<IIrcCommandListener>? value))
        {
            value = [];
            _listeners.Add(cmd.Code, value);
        }

        if (!_container.IsRegistered<TListener>())
        {
            _container.Register<TListener>(Reuse.Singleton);
        }

        value.AddLast(_container.Resolve<TListener>());


        _logger.Information("Registered listener for command {Code}", cmd.Code);
    }

    public NetworkSessionData? GetSessionById(string sessionId)
    {
        return _sessions.FirstOrDefault(s => s.Key == sessionId).Value;
    }

    private async Task ProcessData(string id, MoongateTcpClient client, ReadOnlyMemory<byte> data)
    {
        var messages = await _commandParser.ParseAsync(data);

        foreach (var message in messages)
        {
            await _processQueueService.Enqueue("network", () => DispatchMessage(id, message));
        }
    }

    private async Task DispatchMessage(string id, IIrcCommand command)
    {
        var session = GetSessionById(id);
        if (_listeners.TryGetValue(command.Code, out LinkedList<IIrcCommandListener>? listeners))
        {
            foreach (var listener in listeners)
            {
                await listener.HandleAsync(session, command);
            }
        }
        else
        {
            _logger.Warning("Unknown command listener for {CommandCode}", command.Code);
        }
    }
}
