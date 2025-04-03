using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Server.Core.Data.Directories;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Extensions;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class WelcomeHandler : BaseHandler, IAbyssSignalListener<ClientReadyEvent>
{
    private readonly DirectoriesConfig _directoriesConfig;

    private List<string> _motd;

    private readonly IStringMessageService _stringMessageService;

    private readonly IChannelManagerService _channelManagerService;

    private readonly ITextTemplateService _textTemplateService;

    public WelcomeHandler(
        ILogger<WelcomeHandler> logger,
        DirectoriesConfig directoriesConfig,
        IStringMessageService stringMessageService,
        ITextTemplateService textTemplateService,
        IServiceProvider serviceProvider, IChannelManagerService channelManagerService
    ) : base(logger, serviceProvider)
    {
        _directoriesConfig = directoriesConfig;
        _stringMessageService = stringMessageService;

        _textTemplateService = textTemplateService;
        _channelManagerService = channelManagerService;

        SubscribeSignal(this);

        CheckMOTDFile();
    }


    private void CheckMOTDFile()
    {
        if (ServerConfig.Motd.Motd.StartsWith("file://"))
        {
            var motdFile = Path.Combine(_directoriesConfig.Root, ServerConfig.Motd.Motd.Replace("file://", ""));

            if (!File.Exists(motdFile))
            {
                Logger.LogError("MOTD file not found: {motdFile}", motdFile);

                throw new FileNotFoundException($"MOTD file not found: {motdFile}");
            }

            Logger.LogInformation("MOTD file found: {motdFile}", motdFile);

            _motd = File.ReadAllLines(motdFile).ToList();
        }

        else
        {
            _motd = ServerConfig.Motd.Motd.Split('\n').ToList();
        }
    }


    public async Task OnEventAsync(ClientReadyEvent signalEvent)
    {
        var session = GetSession(signalEvent.Id);


        var operatorCount = GetSessions().Count(s => s.IsOperator);
        var invisibleCount = GetSessions().Count(s => s.IsInvisible);
        var channelsCount = _channelManagerService.Channels.Values.Count(s => !s.IsSecret);

        var welcomeMessage = _stringMessageService.GetMessage(
            new RplWelcome().Code,
            session
        );

        var hostInfo = _stringMessageService.GetMessage(new RplYourHost().Code, session);
        var createdInfo = _stringMessageService.GetMessage(new RplCreated().Code, session);

        await SendIrcMessageAsync(
            signalEvent.Id,
            RplWelcome.CreateWithCustomMessage(Hostname, session.Nickname, welcomeMessage)
        );

        await SendIrcMessageAsync(
            signalEvent.Id,
            RplYourHost.Create(Hostname, session.Nickname, hostInfo)
        );

        await SendIrcMessageAsync(
            signalEvent.Id,
            RplCreated.Create(Hostname, session.Nickname, createdInfo)
        );

        await SendIrcMessageAsync(
            signalEvent.Id,
            RplMyInfo.Create(
                Hostname,
                ServerConfig.Limits.UserModes,
                ServerConfig.Limits.ChannelModes,
                session.Nickname
            )
        );


        await SendIrcMessageAsync(signalEvent.Id, ServerConfig.ToRplSupportCommand(session.Nickname));

        await SendIrcMessageAsync(
            signalEvent.Id,
            RplLuserClient.Create(Hostname, session.Nickname, GetSessions().Count, invisibleCount, 1)
        );


        await SendIrcMessageAsync(signalEvent.Id, RplLuserOp.Create(Hostname, session.Nickname, operatorCount));
        await SendIrcMessageAsync(signalEvent.Id, RplLuserChannels.Create(Hostname, session.Nickname, channelsCount));
        await SendIrcMessageAsync(
            signalEvent.Id,
            RplLocalUsers.Create(
                Hostname,
                session.Nickname,
                GetSessions().Count,
                GetSessionManagerService().MaxSessions
            )
        );

        await SendIrcMessageAsync(
            session.Id,
            RplGlobalUsers.Create(
                Hostname,
                session.Nickname,
                GetSessions().Count,
                GetSessionManagerService().MaxSessions
            )
        );


        await SendIrcMessageAsync(signalEvent.Id, RplLuserMe.Create(Hostname, session.Nickname, GetSessions().Count, 1));
        await SendIrcMessageAsync(signalEvent.Id, RplMotdStart.Create(ServerData.Hostname, session.Nickname));


        foreach (var line in _motd)
        {
            await SendIrcMessageAsync(
                signalEvent.Id,
                new RplMotd(
                    ServerData.Hostname,
                    session.Nickname,
                    _textTemplateService.TranslateText(line, session)
                )
            );
        }

        await SendIrcMessageAsync(signalEvent.Id, new RplEndOfMotd(ServerData.Hostname, session.Nickname));
    }
}
