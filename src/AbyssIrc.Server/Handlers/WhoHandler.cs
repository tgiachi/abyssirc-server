using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Errors;
using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Handlers.Base;

namespace AbyssIrc.Server.Handlers;

public class WhoHandler : BaseHandler, IIrcMessageListener
{
    private readonly IChannelManagerService _channelManagerService;


    public WhoHandler(
        ILogger<WhoHandler> logger, IServiceProvider serviceProvider, IChannelManagerService channelManagerService
    ) : base(logger, serviceProvider)
    {
        _channelManagerService = channelManagerService;
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        if (command is WhoCommand whoCommand)
        {
            return HandleWhoCommandAsync(GetSession(id), whoCommand);
        }

        if (command is WhoIsCommand whoIsCommand)
        {
            return HandleWhoIsCommandAsync(GetSession(id), whoIsCommand);
        }

        return Task.CompletedTask;
    }

    private async Task HandleWhoCommandAsync(IrcSession session, WhoCommand command)
    {
        if (command.IsChannel)
        {
            if (!_channelManagerService.IsChannelRegistered(command.Mask))
            {
                await SendIrcMessageAsync(session.Id, ErrNoSuchChannel.Create(Hostname, session.Nickname, command.Mask));
                return;
            }

            var channel = _channelManagerService.GetChannel(command.Mask);

            foreach (var member in channel.GetMemberList())
            {
                var sessionUser = GetSessionByNickname(member);

                await SendIrcMessageAsync(
                    session.Id,
                    RplWhoReply.Create(
                        Hostname,
                        string.Empty,
                        command.Mask,
                        sessionUser.Username,
                        sessionUser.HostName,
                        Hostname,
                        sessionUser.Nickname,
                        "H",
                        0,
                        sessionUser.RealName
                    )
                );
            }

            await SendIrcMessageAsync(
                session.Id,
                RplEndOfWho.Create(Hostname, session.Nickname, command.Mask)
            );

            return;
        }


        if (command.IsNickname)
        {
            var sessionUser = GetSessionByNickname(command.Mask);

            if (sessionUser == null)
            {
                await SendIrcMessageAsync(session.Id, ErrNoSuchNick.Create(Hostname, session.Nickname, command.Mask));
                return;
            }

            await SendIrcMessageAsync(
                session.Id,
                RplWhoReply.Create(
                    Hostname,
                    string.Empty,
                    "*",
                    sessionUser.Username,
                    sessionUser.HostName,
                    Hostname,
                    sessionUser.Nickname,
                    "H",
                    0,
                    sessionUser.RealName
                )
            );

            await SendIrcMessageAsync(
                session.Id,
                RplEndOfWho.Create(Hostname, session.Nickname, command.Mask)
            );
        }
    }

    private async Task HandleWhoIsCommandAsync(IrcSession session, WhoIsCommand command)
    {
        if (command.Nicknames.Count == 0)
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrNoSuchNick.Create(Hostname, session.Nickname, string.Join(' ', command.Nicknames))
            );
            return;
        }

        foreach (var nickname in command.Nicknames)
        {
            var sessionUser = GetSessionByNickname(nickname);

            if (sessionUser == null)
            {
                await SendIrcMessageAsync(session.Id, ErrNoSuchNick.Create(Hostname, session.Nickname, nickname));
                continue;
            }

            await SendIrcMessageAsync(
                session.Id,
                RplWhoisUser.Create(
                    Hostname,
                    session.Nickname,
                    sessionUser.Nickname,
                    sessionUser.Username,
                    sessionUser.HostName,
                    sessionUser.RealName
                )
            );
        }

        await SendIrcMessageAsync(
            session.Id,
            RplEndOfWho.Create(Hostname, session.Nickname, string.Join(' ', command.Nicknames))
        );
    }
}
