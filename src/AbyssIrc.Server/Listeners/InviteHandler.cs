using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Errors;
using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Listeners.Base;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class InviteHandler : BaseHandler, IIrcMessageListener
{
    private readonly IChannelManagerService _channelManagerService;

    private readonly IStringMessageService _stringMessageService;

    private readonly ITextTemplateService _textTemplateService;

    private const string InviteMessageString = "User {{context.source}} has invited you to join {{context.channel}}";

    public InviteHandler(
        ILogger<InviteHandler> logger, IServiceProvider serviceProvider, IChannelManagerService channelManagerService,
        IStringMessageService stringMessageService, ITextTemplateService textTemplateService
    ) : base(logger, serviceProvider)
    {
        _channelManagerService = channelManagerService;
        _stringMessageService = stringMessageService;
        _textTemplateService = textTemplateService;
    }

    public async Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        var session = GetSession(id);

        if (command is InviteCommand inviteCommand)
        {
            await HandleInviteCommand(session, inviteCommand);
        }
    }

    private async Task HandleInviteCommand(IrcSession session, InviteCommand command)
    {
        if (!_channelManagerService.IsChannelRegistered(command.Channel))
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrNoSuchChannel.Create(
                    Hostname,
                    session.Nickname,
                    command.Channel
                )
            );
            return;
        }

        var channel = _channelManagerService.GetChannel(command.Channel);

        if (channel.IsMember(command.Nickname))
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrUserOnChannel.Create(
                    Hostname,
                    session.Nickname,
                    command.Nickname,
                    command.Channel
                )
            );
            return;
        }

        if (!channel.IsMember(session.Nickname))
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrChanOpPrivsNeeded.Create(
                    Hostname,
                    session.Nickname,
                    command.Channel
                )
            );

            return;
        }

        if (GetSessionManagerService().GetSessionByNickname(command.Nickname) == null)
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrNoSuchNick.Create(
                    Hostname,
                    session.Nickname,
                    command.Nickname
                )
            );

            return;
        }


        channel.AddInvite(session.Nickname);

        await SendIrcMessageAsync(
            session.Id,
            RplInviting.Create(
                Hostname,
                session.Nickname,
                command.Nickname,
                command.Channel
            )
        );

        var targetSession = GetSessionManagerService().GetSessionByNickname(command.Nickname);

        if (targetSession == null)
        {
            return;
        }

        var context = new
        {
            Source = session.Nickname,
            command.Channel
        };

        var message = _stringMessageService.GetMessage(
            "INVITE_NOTICE",
            context
        );

        if (message == null)
        {
            message = _textTemplateService.TranslateText(InviteMessageString, context);
        }

        await SendIrcMessageAsync(
            targetSession.Id,
            NoticeCommand.CreateFromServer(Hostname, targetSession.Nickname, message)
        );
    }
}
