using System.Net;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Servers.Session;
using AbyssIrc.Signals.Interfaces.Services;
using NetCoreServer;

namespace AbyssIrc.Server.Servers;

public class IrcTcpServer : TcpServer
{
    private readonly ITcpService _ircTcpServer;

    private readonly IAbyssIrcSignalEmitterService _signalEmitterService;

    public IrcTcpServer(
        ITcpService tcpService, IAbyssIrcSignalEmitterService signalEmitterService, IPAddress address, int port
    ) : base(address, port)
    {
        _ircTcpServer = tcpService;
        _signalEmitterService = signalEmitterService;
    }

    protected override TcpSession CreateSession()
    {
        return new IrcTcpSession(this);
    }

    public async Task DispatchMessageAsync(string id, string message)
    {
        _ircTcpServer.ParseCommandAsync(id, message);
    }

    public async void ClientConnected(string id, string endPoint)
    {
        await _signalEmitterService.PublishAsync(new ClientConnectedEvent(id, endPoint));
    }

    public async void ClientDisconnected(string id, string endPoint)
    {
        await _signalEmitterService.PublishAsync(new ClientDisconnectedEvent(id, endPoint));
    }
}
