using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Interfaces.Services.System;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class QuitMessageHandler : IIrcMessageListener
{
    private readonly ILogger _logger;
    private readonly ITcpService _tcpService;

    public QuitMessageHandler(ILogger<QuitMessageHandler> logger, ITcpService tcpService)
    {
        _logger = logger;
        _tcpService = tcpService;
    }

    public async Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        if (command is QuitCommand _)
        {
            _tcpService.Disconnect(id);
            _logger.LogInformation("User {Id} has quit the server", id);
        }
    }
}
