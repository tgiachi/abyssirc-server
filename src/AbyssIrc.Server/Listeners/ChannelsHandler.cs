using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Errors;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Data.Channels;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Network.Types;
using AbyssIrc.Server.Data.Internal.Sessions;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Listeners.Base;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class ChannelsHandler : BaseHandler, IIrcMessageListener
{
    private readonly IChannelManagerService _channelManagerService;

    public ChannelsHandler(
        ILogger<ChannelsHandler> logger, IServiceProvider serviceProvider, IChannelManagerService channelManagerService
    ) : base(logger, serviceProvider)
    {
        _channelManagerService = channelManagerService;
    }

    public async Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        var session = GetSession(id);

        if (command is PrivMsgCommand privMsgCommand)
        {
            if (privMsgCommand.IsChannelMessage)
            {
                await HandlePrivMessageChannelMessage(session, privMsgCommand);
            }

            return;
        }

        if (command is JoinCommand joinCommand)
        {
            await HandleJoinMessage(session, joinCommand);
            return;
        }


        if (command is PartCommand partCommand)
        {
            await HandlePartMessage(session, partCommand);
            return;
        }

        if (command is ModeCommand modeCommand)
        {
            if (modeCommand.TargetType == ModeTargetType.Channel)
            {
                await HandleModeMessage(session, modeCommand);
                return;
            }
        }

        if (command is ListCommand listCommand)
        {
            await HandleListCommand(session, listCommand);
            return;
        }

        if (command is NamesCommand namesCommand)
        {
            await HandleNamesCommand(session, namesCommand);
        }
    }

    private async Task HandleModeMessage(IrcSession session, ModeCommand modeCommand)
    {
    }


    private async Task HandlePrivMessageChannelMessage(IrcSession session, PrivMsgCommand command)
    {
        if (_channelManagerService.IsChannelRegistered(command.Target))
        {
            var channelData = _channelManagerService.GetChannelData(command.Target);
            var message = command.Message;

            if (channelData.IsSecret)
            {
                await SendIrcMessageAsync(session.Id, ErrNoSuchNick.Create(Hostname, session.Nickname, command.Target));
                return;
            }

            var sessionsToNotify =
                GetSessionManagerService().GetSessionIdsByNicknames(channelData.GetMemberList().ToArray());

            foreach (var sessionId in sessionsToNotify)
            {
                await SendIrcMessageAsync(
                    sessionId,
                    PrivMsgCommand.CreateToChannel(
                        session.UserMask,
                        channelData.Name,
                        message
                    )
                );
            }
        }
        else
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrNoSuchChannelCommand.Create(Hostname, session.Nickname, command.Target)
            );
        }
    }

    private async Task HandleJoinMessage(IrcSession session, JoinCommand command)
    {
        foreach (var joinData in command.Channels)
        {
            await JoinInChannel(session, joinData);
        }
    }


    private async Task HandlePartMessage(IrcSession session, PartCommand command)
    {
        // _channelManagerService.RemoveNicknameFromChannel(command.ChannelName, session.Nickname);
    }

    private async Task HandleListCommand(IrcSession session, ListCommand command)
    {
        foreach (var channel in command.Channels)
        {
            if (_channelManagerService.IsChannelRegistered(channel))
            {
                await SendListMessage(session, channel);
            }
            else
            {
                await SendIrcMessageAsync(session.Id, ErrNoSuchChannelCommand.Create(Hostname, session.Nickname, channel));
            }
        }
    }


    private async Task HandleNamesCommand(IrcSession session, NamesCommand command)
    {
        foreach (var channel in command.Channels)
        {
            if (_channelManagerService.IsChannelRegistered(channel))
            {
                await SendNamesCommand(session, channel);
            }
            else
            {
                await SendIrcMessageAsync(session.Id, ErrNoSuchChannelCommand.Create(Hostname, session.Nickname, channel));
            }
        }
    }

    private async Task SendNamesCommand(IrcSession session, string channelName)
    {
        var channelData = _channelManagerService.GetChannelData(channelName);
        var nicknames = channelData.GetPrefixedMemberList();

        var message = RplNamReply.Create(
            Hostname,
            session.Nickname,
            channelName,
            nicknames,
            channelData.IsSecret ? ChannelVisibility.Secret : ChannelVisibility.Public
        );
        await SendIrcMessageAsync(session.Id, message);
        await SendIrcMessageAsync(session.Id, RplEndOfNames.Create(Hostname, session.Nickname, channelName));
    }

    private async Task SendListMessage(IrcSession session, string channelName)
    {
        await SendIrcMessageAsync(session.Id, RplListStart.Create(Hostname, session.Nickname));

        var channelData = _channelManagerService.GetChannelData(channelName);
        var message = RplList.Create(
            Hostname,
            session.Nickname,
            channelName,
            channelData.MemberCount,
            channelData.Topic
        );

        await SendIrcMessageAsync(session.Id, message);

        await SendIrcMessageAsync(session.Id, RplListEnd.Create(Hostname, session.Nickname));
    }

    private async Task JoinInChannel(IrcSession session, JoinChannelData joinChannelData)
    {
    }
}
