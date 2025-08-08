using System.Net;
using System.Text;
using AbyssIrc.Protocol.Messages.Interfaces.Parser;
using AbyssIrc.Server.Core.Data.Config;
using AbyssIrc.Server.Core.Interfaces.Services;
using AbyssIrc.Server.Network.Data;
using AbyssIrc.Server.Network.Servers.Tcp;
using NanoidDotNet;
using Serilog;

namespace AbyssIrc.Server.Services;

public class NetworkService : INetworkService
{
    private readonly AbyssIrcServerConfig _abyssIrcServerConfig;
    private readonly ILogger _logger = Log.ForContext<NetworkService>();

    private readonly IProcessQueueService _processQueueService;
    private readonly IIrcCommandParser _commandParser;


    private readonly Dictionary<string, ServerDataObject> _servers = new();

    public NetworkService(
        AbyssIrcServerConfig abyssIrcServerConfig, IProcessQueueService processQueueService, IIrcCommandParser commandParser
    )
    {
        _abyssIrcServerConfig = abyssIrcServerConfig;
        _processQueueService = processQueueService;
        _commandParser = commandParser;


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
    }

    private void OnClientDisconnected(string id, MoongateTcpClient moonTcpClient)
    {
        _logger.Information("Client disconnected");
    }

    private void OnServerError(string id, Exception ex)
    {
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
    }

    private async Task ProcessData(string id, MoongateTcpClient client, ReadOnlyMemory<byte> data)
    {
        _commandParser.ParseAsync(Encoding.UTF8.GetString(data.Span));
    }
}
