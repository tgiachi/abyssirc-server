using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Events.Commands;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class QuitMessageHandler : BaseHandler, IIrcMessageListener, IAbyssSignalListener<QuitRequestEvent>
{
    private readonly ILogger _logger;
    private readonly ITcpService _tcpService;

    private readonly IChannelManagerService _channelManagerService;


    public QuitMessageHandler(
        ILogger<QuitMessageHandler> logger, IServiceProvider serviceProvider, ITcpService tcpService,
        IChannelManagerService channelManagerService
    ) : base(logger, serviceProvider)
    {
        _logger = logger;
        _tcpService = tcpService;
        _channelManagerService = channelManagerService;

        SubscribeSignal(this);
    }

    public async Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        if (command is QuitCommand quitCommand)
        {
            var session = GetSession(id);
            await HandleQuitMessage(session, quitCommand);
            _logger.LogInformation("User {Id} has quit the server", id);
        }
    }

    private async Task HandleQuitMessage(IrcSession session, QuitCommand quitCommand)
    {
        _logger.LogInformation("User {Id} has quit the server", session.Id);

        // Remove user from all channels
        foreach (var channel in _channelManagerService.GetChannelsOfNickname(session.Nickname))
        {
            _channelManagerService.RemoveNicknameFromChannel(channel.Name, session.Nickname);

            var sessionToNotify = GetSessionManagerService()
                .GetSessionIdsByNicknames(_channelManagerService.GetNicknamesInChannel(channel.Name).ToArray());

            foreach (var sessionId in sessionToNotify)
            {
                await _tcpService.SendIrcMessagesAsync(
                    sessionId,
                    PartCommand.CreateForChannel(session.UserMask, channel.Name, quitCommand.Message)
                );
            }
        }

        // Remove user from the session manager
        _tcpService.Disconnect(session.Id);
    }

    public async Task OnEventAsync(QuitRequestEvent signalEvent)
    {
        var session = GetSession(signalEvent.SessionId);
        var quitCommand = QuitCommand.CreateNotification(session.UserMask, signalEvent.Reason);

        await HandleQuitMessage(session, quitCommand);
    }
}
