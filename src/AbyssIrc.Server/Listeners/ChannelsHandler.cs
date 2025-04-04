using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Errors;
using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Protocol.Messages.Data.Channels;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Protocol.Messages.Types;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Events.Channels;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Data.Events.Sessions;
using AbyssIrc.Server.Data.Events.TcpServer;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class ChannelsHandler
    : BaseHandler, IIrcMessageListener, IAbyssSignalListener<SessionRemovedEvent>,
        IAbyssSignalListener<AddUserJoinChannelEvent>
{
    private readonly IChannelManagerService _channelManagerService;


    public ChannelsHandler(
        ILogger<ChannelsHandler> logger, IServiceProvider serviceProvider, IChannelManagerService channelManagerService
    ) : base(logger, serviceProvider)
    {
        _channelManagerService = channelManagerService;
        SubscribeSignal<SessionRemovedEvent>(this);
        SubscribeSignal<AddUserJoinChannelEvent>(this);
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
            return;
        }

        if (command is TopicCommand topicCommand)
        {
            await HandleTopicMessage(session, topicCommand);
            return;
        }

        if (command is KickCommand kickCommand)
        {
            await HandleKickCommand(session, kickCommand);
            return;
        }
    }


    private async Task HandleKickCommand(IrcSession session, KickCommand command)
    {
        if (_channelManagerService.IsChannelRegistered(command.Channel))
        {
            var channelData = _channelManagerService.GetChannel(command.Channel);

            if (!channelData.IsOperator(session.Nickname))
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrChanOpPrivsNeeded.Create(Hostname, session.Nickname, command.Channel)
                );
                return;
            }

            if (!channelData.IsMember(command.Target))
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrNotOnChannel.Create(Hostname, session.Nickname, command.Channel)
                );
                return;
            }

            if (channelData.IsMember(command.Target))
            {
                var sessionsToNotify =
                    GetSessionManagerService()
                        .GetSessionIdsByNicknames(channelData.GetMemberList().ToArray());

                _channelManagerService.RemoveNicknameFromChannel(command.Channel, command.Target);

                foreach (var sessionId in sessionsToNotify)
                {
                    await SendIrcMessageAsync(
                        sessionId,
                        KickCommand.Create(
                            session.UserMask,
                            command.Channel,
                            command.Target,
                            command.Reason
                        )
                    );

                    await SendListMessage(GetSession(sessionId), command.Channel);
                }
            }


            return;
        }

        await SendIrcMessageAsync(
            session.Id,
            ErrNoSuchChannel.Create(Hostname, session.Nickname, command.Channel)
        );
    }

    // private async Task HandleModeMessage(IrcSession session, ModeCommand command)
    // {
    //     if (_channelManagerService.IsChannelRegistered(command.Target))
    //     {
    //         var channelData = _channelManagerService.GetChannelData(command.Target);
    //
    //         if (command.ModeChanges.Count > 0)
    //         {
    //             if (channelData.IsOperator(session.Nickname))
    //             {
    //                 var modesChanges = new List<ModeChangeType>();
    //
    //                 foreach (var modeChange in command.ModeChanges)
    //                 {
    //                     if (modeChange.IsAdding)
    //                     {
    //                         channelData.SetMode(modeChange.Mode);
    //                         modesChanges.Add(modeChange);
    //                     }
    //                     else
    //                     {
    //                         channelData.RemoveMode(modeChange.Mode);
    //                         modesChanges.Add(new ModeChangeType(false, modeChange.Mode));
    //                     }
    //                 }
    //
    //                 if (modesChanges.Count > 0)
    //                 {
    //                     var sessionsToNotify =
    //                         GetSessionManagerService()
    //                             .GetSessionIdsByNicknames(channelData.GetMemberList().ToArray());
    //
    //                     foreach (var sessionId in sessionsToNotify)
    //                     {
    //                         await SendIrcMessageAsync(
    //                             sessionId,
    //                             ModeCommand.CreateWithModes(
    //                                 Hostname,
    //                                 channelData.Name,
    //                                 modesChanges.ToArray()
    //                             )
    //                         );
    //                     }
    //                 }
    //
    //                 return;
    //             }
    //
    //             await SendIrcMessageAsync(
    //                 session.Id,
    //                 ErrChanOpPrivsNeeded.Create(Hostname, session.Nickname, command.Target)
    //             );
    //             return;
    //         }
    //
    //
    //         await SendIrcMessageAsync(
    //             session.Id,
    //             ModeCommand.CreateWithModes(
    //                 Hostname,
    //                 channelData.Name,
    //                 channelData.GetModeChanges()
    //             )
    //         );
    //     }
    //     else
    //     {
    //         await SendIrcMessageAsync(
    //             session.Id,
    //             ErrNoSuchChannelCommand.Create(Hostname, session.Nickname, command.Target)
    //         );
    //     }
    // }

    /// <summary>
    /// Handles a MODE message from a client
    /// </summary>
    /// <param name="session">The client session</param>
    /// <param name="command">The MODE command</param>
    private async Task HandleModeMessage(IrcSession session, ModeCommand command)
    {
        // Check if it's a channel mode command
        if (command.TargetType == ModeTargetType.Channel)
        {
            await HandleChannelModeCommand(session, command);
            await BroadcastNamesCommand(command.Target);
        }
        else
        {
            await HandleUserModeCommand(session, command);
        }
    }

    /// <summary>
    /// Handles channel mode commands
    /// </summary>
    private async Task HandleChannelModeCommand(IrcSession session, ModeCommand command)
    {
        // Check if channel exists
        if (!_channelManagerService.IsChannelRegistered(command.Target))
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrNoSuchChannel.Create(Hostname, session.Nickname, command.Target)
            );
            return;
        }

        var channelData = _channelManagerService.GetChannel(command.Target);

        // No mode changes - query mode
        if (command.ModeChanges.Count == 0)
        {
            await SendIrcMessageAsync(
                session.Id,
                ModeCommand.CreateWithModes(
                    Hostname,
                    channelData.Name,
                    channelData.GetModeChanges()
                )
            );
            return;
        }

        // Attempt to change modes
        if (!channelData.IsOperator(session.Nickname))
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrChanOpPrivsNeeded.Create(Hostname, session.Nickname, command.Target)
            );
            return;
        }

        // Process mode changes
        var processedChanges = ProcessChannelModeChanges(session, channelData, command.ModeChanges);


        foreach (var changed in processedChanges)
        {
            if (changed.Mode == 'k')
            {
                Logger.LogDebug(
                    "User {Nickname} set channel key to {Channel} - ({Key}) ",
                    session.Nickname,
                    channelData.Name,
                    channelData.Key
                );
            }
        }

        // If we processed any changes, notify channel members
        if (processedChanges.Count > 0)
        {
            await NotifyChannelModeChanges(channelData, processedChanges);
        }
    }

    /// <summary>
    /// Process channel mode changes and apply them to the channel
    /// </summary>
    private List<ModeChangeType> ProcessChannelModeChanges(
        IrcSession session, ChannelData channelData, List<ModeChangeType> modeChanges
    )
    {
        var processedChanges = new List<ModeChangeType>();

        foreach (var change in modeChanges)
        {
            if (change.IsAdding)
            {
                // Handle special modes with parameters
                if (change.Mode == 'o' && !string.IsNullOrEmpty(change.Parameter))
                {
                    channelData.SetOperator(change.Parameter, true);
                    processedChanges.Add(change);
                }

                if (change.Mode == 'v' && !string.IsNullOrEmpty(change.Parameter))
                {
                    channelData.SetVoice(change.Parameter, true);
                    processedChanges.Add(change);
                }

                if (change.Mode == 'b' && !string.IsNullOrEmpty(change.Parameter))
                {
                    channelData.AddBan(change.Parameter, session.Nickname);
                    processedChanges.Add(change);
                }

                if (change.Mode == 'k' && !string.IsNullOrEmpty(change.Parameter))
                {
                    channelData.SetMode('k', change.Parameter);
                    processedChanges.Add(change);
                }
                else
                {
                    // Standard channel mode
                    channelData.SetMode(change.Mode);
                    processedChanges.Add(change);
                }
            }
            else
            {
                // Handle mode removal
                if (change.Mode == 'o' && !string.IsNullOrEmpty(change.Parameter))
                {
                    channelData.SetOperator(change.Parameter, false);
                    processedChanges.Add(change);
                }

                if (change.Mode == 'v' && !string.IsNullOrEmpty(change.Parameter))
                {
                    channelData.SetVoice(change.Parameter, false);
                    processedChanges.Add(change);
                }

                if (change.Mode == 'b' && !string.IsNullOrEmpty(change.Parameter))
                {
                    channelData.RemoveBan(change.Parameter);
                    processedChanges.Add(change);
                }

                if (change.Mode == 'k' && !string.IsNullOrEmpty(change.Parameter))
                {
                    channelData.RemoveMode('k');
                    processedChanges.Add(change);
                }
                else
                {
                    // Standard channel mode
                    channelData.RemoveMode(change.Mode);
                    processedChanges.Add(change);
                }
            }
        }

        return processedChanges;
    }

    /// <summary>
    /// Notify all members of a channel about mode changes
    /// </summary>
    private async Task NotifyChannelModeChanges(ChannelData channelData, List<ModeChangeType> modeChanges)
    {
        var sessionsToNotify = GetSessionManagerService()
            .GetSessionIdsByNicknames(channelData.GetMemberList().ToArray());

        foreach (var sessionId in sessionsToNotify)
        {
            await SendIrcMessageAsync(
                sessionId,
                ModeCommand.CreateWithModes(
                    Hostname,
                    channelData.Name,
                    modeChanges.ToArray()
                )
            );
        }
    }

    /// <summary>
    /// Handles user mode commands
    /// </summary>
    private async Task HandleUserModeCommand(IrcSession session, ModeCommand command)
    {
        // Only users can change their own modes
        if (session.Nickname != command.Target)
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrUsersDontMatch.Create(Hostname, session.Nickname)
            );
            return;
        }

        // Process the mode changes
        var processedChanges = new List<ModeChangeType>();

        foreach (var change in command.ModeChanges)
        {
            if (change.IsAdding)
            {
                // Don't allow users to make themselves operators
                if (change.Mode != 'o')
                {
                    session.AddMode(change.Mode);
                    processedChanges.Add(change);
                }
            }
            else
            {
                session.RemoveMode(change.Mode);
                processedChanges.Add(change);
            }
        }

        // Notify the user about their mode changes
        if (processedChanges.Count > 0)
        {
            await SendIrcMessageAsync(
                session.Id,
                ModeCommand.CreateWithModes(
                    Hostname,
                    session.Nickname,
                    processedChanges.ToArray()
                )
            );
        }
    }

    private async Task HandleTopicMessage(IrcSession session, TopicCommand topicCommand)
    {
        if (_channelManagerService.IsChannelRegistered(topicCommand.Channel))
        {
            var channelData = _channelManagerService.GetChannel(topicCommand.Channel);
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
                ErrNoSuchChannel.Create(Hostname, session.Nickname, topicCommand.Channel)
            );
        }
    }


    private async Task HandlePrivMessageChannelMessage(IrcSession session, PrivMsgCommand command)
    {
        if (_channelManagerService.IsChannelRegistered(command.Target))
        {
            var channelData = _channelManagerService.GetChannel(command.Target);
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
                ErrNoSuchChannel.Create(Hostname, session.Nickname, command.Target)
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
                var channelData = _channelManagerService.GetChannel(channelName);
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
                    ErrNoSuchChannel.Create(Hostname, session.Nickname, channelName)
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
                        ErrNoSuchChannel.Create(Hostname, session.Nickname, channel)
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
                await SendIrcMessageAsync(session.Id, ErrNoSuchChannel.Create(Hostname, session.Nickname, channel));
            }
        }
    }

    private async Task BroadcastNamesCommand(string channelName)
    {
        var channelData = _channelManagerService.GetChannel(channelName);
        foreach (var sessionId in GetSessionManagerService().GetSessionIdsByNicknames(channelData.GetMemberList().ToArray()))
        {
            await SendNamesCommand(GetSession(sessionId), channelName);
        }
    }

    private async Task SendNamesCommand(IrcSession session, string channelName)
    {
        var channelData = _channelManagerService.GetChannel(channelName);

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

        var channelData = _channelManagerService.GetChannel(channelName);
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
            channelData = _channelManagerService.GetChannel(joinChannelData.ChannelName);

            if (channelData.HasKey && channelData.Key != joinChannelData.Key)
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrBadChannelKey.Create(Hostname, session.Nickname, joinChannelData.ChannelName)
                );
                return;
            }


            if (channelData.IsInviteOnly && !channelData.NickNameCanJoin(session.Nickname))
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrInviteOnlyChan.Create(Hostname, session.Nickname, joinChannelData.ChannelName)
                );
                return;
            }


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
            channelData = _channelManagerService.GetChannel(joinChannelData.ChannelName);
            channelData.SetOperator(session.Nickname, true);
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

        await SendNamesCommand(session, channelData.Name);

        await SendTopicToUser(session, channelData);

        await SendIrcMessageAsync(
            session.Id,
            RplCreationTime.Create(Hostname, session.Nickname, joinChannelData.ChannelName, channelData.CreationTime)
        );
        await SendIrcMessageAsync(
            session.Id,
            RplChannelModeIs.Create(Hostname, session.Nickname, channelData.Name, channelData.GetModeString())
        );


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

    /// <summary>
    /// Parses a MODE command string to extract target, mode changes, and parameters
    /// </summary>
    /// <param name="modeString">The complete mode command string</param>
    /// <returns>A ModeCommand object representing the parsed command</returns>
    private static ModeCommand ParseModeCommand(string modeString)
    {
        // Split the string into parts
        string[] parts = modeString.Split(' ');

        // Ensure we have at least the command and target
        if (parts.Length < 3 || !parts[0].Equals("MODE", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Create a new MODE command
        ModeCommand modeCommand = new ModeCommand
        {
            Target = parts[1]
        };

        // Parse the mode string (e.g., "+o")
        string modeChanges = parts[2];

        bool isAdding = true;
        List<char> modes = new List<char>();

        // Process each character in the mode string
        foreach (char c in modeChanges)
        {
            if (c == '+')
            {
                isAdding = true;
            }
            else if (c == '-')
            {
                isAdding = false;
            }
            else
            {
                modes.Add(c);
            }
        }

        // Process the modes with their parameters (if any)
        int paramIndex = 3;
        foreach (char mode in modes)
        {
            ModeChangeType modeChange = new ModeChangeType
            {
                IsAdding = isAdding,
                Mode = mode
            };

            // Check if this mode requires a parameter and if we have parameters available
            if (NeedsParameter(mode, modeCommand.TargetType) && paramIndex < parts.Length)
            {
                modeChange.Parameter = parts[paramIndex++];
            }

            modeCommand.ModeChanges.Add(modeChange);
        }

        return modeCommand;
    }

    /// <summary>
    /// Determines if a specific mode requires a parameter
    /// </summary>
    private static bool NeedsParameter(char mode, ModeTargetType targetType)
    {
        // Common channel modes that require parameters
        if (targetType == ModeTargetType.Channel)
        {
            // o: op, v: voice, b: ban, k: key, l: limit, etc.
            return "ovbklIe".Contains(mode);
        }

        // Common user modes that require parameters (most don't)
        return false;
    }


    /// <summary>
    ///  When the client disconnects, we need to remove the user from all channels
    /// </summary>
    /// <param name="signalEvent"></param>
    /// <returns></returns>
    public async Task OnEventAsync(SessionRemovedEvent signalEvent)
    {
        var session = signalEvent.Session;

        if (!session.IsValid)
        {
            Logger.LogWarning("Session is not valid: {SessionId}", session.Id);
            return;
        }

        foreach (var channel in _channelManagerService.Channels.Values)
        {
            if (channel.IsMember(session.Nickname))
            {
                _channelManagerService.RemoveNicknameFromChannel(channel.Name, session.Nickname);

                var sessionsToNotify =
                    GetSessionManagerService()
                        .GetSessionIdsByNicknames(channel.GetMemberList().ToArray());

                foreach (var sessionId in sessionsToNotify)
                {
                    await SendIrcMessageAsync(
                        sessionId,
                        PartCommand.CreateForChannel(session.UserMask, channel.Name, "Client disconnected")
                    );
                }
            }
        }
    }

    public async Task OnEventAsync(AddUserJoinChannelEvent signalEvent)
    {
        Logger.LogDebug(
            "Received AddUserJoinChannelEvent: Nickname: {Nickname} to {Channel}",
            signalEvent.Nickname,
            signalEvent.Channel
        );

        var session = GetSessionByNickname(signalEvent.Nickname);

        if (session == null)
        {
            Logger.LogWarning("Session not found for nickname: {Nickname}", signalEvent.Nickname);
            return;
        }

        await HandleJoinMessage(session, JoinCommand.Create(signalEvent.Channel));
    }
}
