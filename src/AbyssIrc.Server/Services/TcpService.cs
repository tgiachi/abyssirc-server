using System.Net;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Servers;
using AbyssIrc.Signals.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Services;

public class TcpService : ITcpService
{
    private readonly AbyssIrcConfig _abyssIrcConfig;
    private readonly ILogger _logger = Log.ForContext<TcpService>();

    private readonly IIrcCommandParser _commandParser;
    private readonly IAbyssIrcSignalEmitterService _signalEmitterService;

    private readonly Dictionary<int, IrcTcpServer> _servers = new();
    private readonly IIrcManagerService _ircManagerService;

    public TcpService(
        AbyssIrcConfig abyssIrcConfig, IIrcCommandParser commandParser,
        IIrcManagerService ircManagerService, IAbyssIrcSignalEmitterService signalEmitterService
    )
    {
        _abyssIrcConfig = abyssIrcConfig;
        _commandParser = commandParser;

        _ircManagerService = ircManagerService;
        _signalEmitterService = signalEmitterService;
    }

    public async Task StartAsync()
    {
        _logger.Information("Starting TCP service");


        _logger.Information("Server listening on port {Port}", _abyssIrcConfig.Network.Port);
        _servers.Add(
            _abyssIrcConfig.Network.Port,
            new IrcTcpServer(this, _signalEmitterService, IPAddress.Any, _abyssIrcConfig.Network.Port)
        );

        if (!string.IsNullOrEmpty(_abyssIrcConfig.Network.SslCertPath))
        {
            _logger.Information("Server listening on port {Port}", _abyssIrcConfig.Network.SslPort);
            _servers.Add(
                _abyssIrcConfig.Network.SslPort,
                new IrcTcpServer(this, _signalEmitterService, IPAddress.Any, _abyssIrcConfig.Network.SslPort)
            );
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

    public async Task ParseCommandAsync(string sessionId, string command)
    {
        var parsedCommands = await _commandParser.ParseAsync(command);

        foreach (var parsedCommand in parsedCommands)
        {
            await _ircManagerService.DispatchMessageAsync(sessionId, parsedCommand);
        }
    }

    public async Task SendMessagesAsync(string sessionId, List<string> messages)
    {
        var outputMessage = string.Join("\r\n", messages);
        foreach (var value in _servers.Values)
        {
            var tcpSession = value.FindSession(Guid.Parse(sessionId));

            if (tcpSession != null)
            {
                tcpSession.SendAsync(outputMessage);
            }
        }
    }
}
