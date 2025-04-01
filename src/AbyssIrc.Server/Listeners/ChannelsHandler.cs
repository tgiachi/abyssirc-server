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

        if (command is TopicCommand topicCommand)
        {
            await HandleTopicMessage(session, topicCommand);
            return;
        }
    }

    private async Task HandleModeMessage(IrcSession session, ModeCommand command)
    {
        if (_channelManagerService.IsChannelRegistered(command.Target))
        {
            var channelData = _channelManagerService.GetChannelData(command.Target);

            if (command.ModeChanges.Count > 0)
            {
                if (channelData.IsOperator(session.Nickname))
                {
                    var modesChanges = new List<ModeChangeType>();

                    foreach (var modeChange in command.ModeChanges)
                    {
                        if (modeChange.IsAdding)
                        {
                            channelData.SetMode(modeChange.Mode);
                            modesChanges.Add(modeChange);
                        }
                        else
                        {
                            channelData.RemoveMode(modeChange.Mode);
                            modesChanges.Add(new ModeChangeType(false, modeChange.Mode));
                        }
                    }

                    if (modesChanges.Count > 0)
                    {
                        var sessionsToNotify =
                            GetSessionManagerService()
                                .GetSessionIdsByNicknames(channelData.GetMemberList().ToArray());

                        foreach (var sessionId in sessionsToNotify)
                        {
                            await SendIrcMessageAsync(
                                sessionId,
                                ModeCommand.CreateWithModes(
                                    Hostname,
                                    session.Nickname,
                                    modesChanges.ToArray()
                                )
                            );
                        }
                    }

                    return;
                }
                else
                {
                    await SendIrcMessageAsync(
                        session.Id,
                        ErrChanOpPrivsNeeded.Create(Hostname, session.Nickname, command.Target)
                    );
                    return;
                }
            }



            await SendIrcMessageAsync(
                session.Id,
                ModeCommand.CreateWithModes(
                    Hostname,
                    channelData.Name,
                    channelData.GetModeChanges()
                )
            );
        }
        else
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrNoSuchChannelCommand.Create(Hostname, session.Nickname, command.Target)
            );
        }
    }

    private async Task HandleTopicMessage(IrcSession session, TopicCommand topicCommand)
    {
        if (_channelManagerService.IsChannelRegistered(topicCommand.Channel))
        {
            var channelData = _channelManagerService.GetChannelData(topicCommand.Channel);
            if (channelData.IsSecret)
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrNoSuchNick.Create(Hostname, session.Nickname, topicCommand.Channel)
                );
                return;
            }

            channelData.TopicSetTime = DateTime.Now;
            channelData.TopicSetBy = session.Nickname;
            channelData.Topic = topicCommand.Topic;

            var sessionsToNotify =
                GetSessionManagerService()
                    .GetSessionIdsByNicknames(channelData.GetMemberList().ToArray());

            foreach (var sessionId in sessionsToNotify)
            {
                await SendIrcMessageAsync(
                    sessionId,
                    RplTopic.Create(Hostname, session.Nickname, channelData.Name, channelData.Topic)
                );
            }
        }
        else
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrNoSuchChannelCommand.Create(Hostname, session.Nickname, topicCommand.Channel)
            );
        }
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
                GetSessionManagerService()
                    .GetSessionIdsByNicknames(channelData.GetMemberList().Where(s => s != session.Nickname).ToArray());

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
        if (command.Channels.Count > ServerConfig.Limits.MaxChanJoin)
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrTooManyChannels.Create(Hostname, session.Nickname, command.Channels.First().ChannelName)
            );
            return;
        }

        foreach (var joinData in command.Channels)
        {
            if (joinData.IsValid)
            {
                await JoinInChannel(session, joinData);
            }
            else
            {
                Logger.LogWarning(
                    "Invalid channel name: {ChannelName} from user: {Nickname}",
                    joinData.ChannelName,
                    session.Nickname
                );
            }
        }
    }

    private async Task HandlePartMessage(IrcSession session, PartCommand command)
    {
        foreach (var channelName in command.Channels)
        {
            if (_channelManagerService.IsChannelRegistered(channelName))
            {
                var channelData = _channelManagerService.GetChannelData(channelName);
                if (channelData.IsMember(session.Nickname))
                {
                    _channelManagerService.RemoveNicknameFromChannel(channelName, session.Nickname);

                    var sessionsToNotify =
                        GetSessionManagerService()
                            .GetSessionIdsByNicknames(channelData.GetMemberList().ToArray());

                    sessionsToNotify.Add(session.Id);

                    foreach (var sessionId in sessionsToNotify)
                    {
                        await SendIrcMessageAsync(
                            sessionId,
                            PartCommand.CreateForChannel(
                                session.UserMask,
                                channelName,
                                command.PartMessage
                            )
                        );
                    }
                }
                else
                {
                    await SendIrcMessageAsync(
                        session.Id,
                        ErrNotOnChannel.Create(Hostname, session.Nickname, channelName)
                    );
                }
            }
            else
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrNoSuchChannelCommand.Create(Hostname, session.Nickname, channelName)
                );
            }
        }
    }

    private async Task HandleListCommand(IrcSession session, ListCommand command)
    {
        if (command.Channels.Count > 0)
        {
            foreach (var channel in command.Channels)
            {
                if (_channelManagerService.IsChannelRegistered(channel))
                {
                    await SendListMessage(session, channel);
                }
                else
                {
                    await SendIrcMessageAsync(
                        session.Id,
                        ErrNoSuchChannelCommand.Create(Hostname, session.Nickname, channel)
                    );
                }
            }
        }
        else
        {
            await SendAllChannelList(session, command);
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

        var message = RplNameReply.Create(
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

    private async Task SendAllChannelList(IrcSession session, ListCommand command)
    {
        await SendIrcMessageAsync(session.Id, RplListStart.Create(Hostname, session.Nickname));

        var messages = new List<IIrcCommand>();

        _channelManagerService.Channels.Values.Where(s => !s.IsSecret)
            .ToList()
            .ForEach(
                channelData =>
                {
                    messages.Add(
                        RplList.Create(
                            Hostname,
                            session.Nickname,
                            channelData.Name,
                            channelData.MemberCount,
                            channelData.Topic
                        )
                    );
                }
            );

        foreach (var message in messages)
        {
            await SendIrcMessageAsync(session.Id, message);
        }


        await SendIrcMessageAsync(session.Id, RplListEnd.Create(Hostname, session.Nickname));
    }

    private async Task JoinInChannel(IrcSession session, JoinChannelData joinChannelData)
    {
        ChannelData channelData;
        bool isNewChannel = false;
        if (_channelManagerService.IsChannelRegistered(joinChannelData.ChannelName))
        {
            channelData = _channelManagerService.GetChannelData(joinChannelData.ChannelName);
            if (channelData.IsSecret)
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrNoSuchNick.Create(Hostname, session.Nickname, joinChannelData.ChannelName)
                );
                return;
            }

            if (channelData.IsMember(session.Nickname))
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrAlreadyInChannel.Create(Hostname, session.Nickname, session.Nickname, channelData.Name)
                );
                return;
            }

            _channelManagerService.AddNicknameToChannel(joinChannelData.ChannelName, session.Nickname);
        }
        else
        {
            isNewChannel = true;
            _channelManagerService.RegisterChannel(joinChannelData.ChannelName);
            _channelManagerService.AddNicknameToChannel(joinChannelData.ChannelName, session.Nickname);
            channelData = _channelManagerService.GetChannelData(joinChannelData.ChannelName);
            channelData.SetOperator(session.Nickname, true);
            channelData.SetTopicProtection();
            channelData.SetAntiSpamControl();
            channelData.SetOnlyForPresents();
        }

        // Notify other members
        var sessionsToNotify =
            GetSessionManagerService().GetSessionIdsByNicknames(channelData.GetMemberList().ToArray());

        foreach (var sessionId in sessionsToNotify)
        {
            await SendIrcMessageAsync(
                sessionId,
                JoinCommand.CreateForChannels(
                    session.UserMask,
                    channelData.Name
                )
            );
        }

        await SendTopicToUser(session, channelData);

        await SendIrcMessageAsync(
            session.Id,
            RplCreationTime.Create(Hostname, session.Nickname, joinChannelData.ChannelName, channelData.CreationTime)
        );
        await SendIrcMessageAsync(
            session.Id,
            RplChannelModeIs.Create(Hostname, session.Nickname, channelData.Name, channelData.GetModeString())
        );


        await SendNamesCommand(session, channelData.Name);

        if (isNewChannel)
        {
            await SendIrcMessageAsync(
                session.Id,
                ModeCommand.CreateWithModes(Hostname, channelData.Name, new ModeChangeType(true, 'o', session.Nickname))
            );
        }
    }

    private async Task SendTopicToUser(IrcSession session, ChannelData channelData)
    {
        if (channelData.HaveTopic)
        {
            await SendIrcMessageAsync(
                session.Id,
                RplTopic.Create(Hostname, session.Nickname, channelData.Name, channelData.Topic)
            );

            await SendIrcMessageAsync(
                session.Id,
                RplTopicWhoTime.Create(
                    Hostname,
                    session.Nickname,
                    channelData.Name,
                    channelData.TopicSetBy,
                    channelData.TopicSetTime
                )
            );
        }
        else
        {
            await SendIrcMessageAsync(
                session.Id,
                RplNoTopic.Create(Hostname, session.Nickname, channelData.Name)
            );
        }
    }
}
