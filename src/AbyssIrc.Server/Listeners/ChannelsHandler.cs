using AbyssIrc.Network.Commands;
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
                await HandleChannelMessage(session, privMsgCommand);
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
    }

    private async Task HandleModeMessage(IrcSession session, ModeCommand modeCommand)
    {
    }


    private async Task HandleChannelMessage(IrcSession session, PrivMsgCommand command)
    {
    }

    private async Task HandleJoinMessage(IrcSession session, JoinCommand command)
    {
        // if (_channelManagerService.IsChannelRegistered(command.ChannelName))
        // {
        //     _channelManagerService.AddNicknameToChannel(command.ChannelName, session.Nickname);
        // }
        // else
        // {
        //     _channelManagerService.RegisterChannel(command.ChannelName);
        //     _channelManagerService.AddNicknameToChannel(command.ChannelName, session.Nickname);
        // }
    }

    private async Task HandlePartMessage(IrcSession session, PartCommand command)
    {
        // _channelManagerService.RemoveNicknameFromChannel(command.ChannelName, session.Nickname);
    }
}
