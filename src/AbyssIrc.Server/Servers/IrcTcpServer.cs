using System.Net;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Core.Types;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Servers.Session;
using AbyssIrc.Signals.Interfaces.Services;
using NetCoreServer;

namespace AbyssIrc.Server.Servers;

public class IrcTcpServer : TcpServer
{
    private readonly ITcpService _ircTcpServer;

    private readonly IAbyssSignalService _signalService;

    private readonly ISessionManagerService _sessionManagerService;

    public IrcTcpServer(
        ITcpService tcpService, ISessionManagerService sessionManagerService, IAbyssSignalService signalService,
        IPAddress address, int port
    ) : base(address, port)
    {
        _ircTcpServer = tcpService;
        _signalService = signalService;
        _sessionManagerService = sessionManagerService;

        OptionNoDelay = true;
        OptionReceiveBufferSize = 8192;
        OptionSendBufferSize = 8192;
    }


    protected override TcpSession CreateSession()
    {
        return new IrcTcpSession(this);
    }

    public async Task DispatchMessageAsync(string id, string message)
    {
        _ircTcpServer.ParseCommandAsync(TcpServerType.Plain, id, message);
    }

    public async void ClientConnected(string id, string endPoint)
    {
        _sessionManagerService.AddSession(id, endPoint);
        await _signalService.PublishAsync(new ClientConnectedEvent(id, endPoint));
    }

    public async void ClientDisconnected(string id, string endPoint)
    {
        await _signalService.PublishAsync(new ClientDisconnectedEvent(id, endPoint));
    }
}
