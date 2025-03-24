using System.Net;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Server.Interfaces;
using AbyssIrc.Server.Servers;
using Serilog;

namespace AbyssIrc.Server.Services;

public class TcpService : ITcpService
{
    private readonly AbyssIrcConfig _abyssIrcConfig;
    private readonly ILogger _logger = Log.ForContext<TcpService>();

    private readonly Dictionary<int, IrcTcpServer> _servers = new();

    public TcpService(AbyssIrcConfig abyssIrcConfig)
    {
        _abyssIrcConfig = abyssIrcConfig;
    }

    public async Task StartAsync()
    {
        _logger.Information("Starting TCP service");


        _logger.Information("Server listening on port {Port}", _abyssIrcConfig.Network.Port);
        _servers.Add(_abyssIrcConfig.Network.Port, new IrcTcpServer(IPAddress.Any, _abyssIrcConfig.Network.Port));

        if (!string.IsNullOrEmpty(_abyssIrcConfig.Network.SslCertPath))
        {
            _logger.Information("Server listening on port {Port}", _abyssIrcConfig.Network.SslPort);
            _servers.Add(_abyssIrcConfig.Network.SslPort, new IrcTcpServer(IPAddress.Any, _abyssIrcConfig.Network.SslPort));
        }

        foreach (var server in _servers.Values)
        {
            server.Start();
        }
    }

    public async Task StopAsync()
    {
        foreach (var server in _servers.Values)
        {
            server.Stop();
        }

        _servers.Clear();
    }
}
