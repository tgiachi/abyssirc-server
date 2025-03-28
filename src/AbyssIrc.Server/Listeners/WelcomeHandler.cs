using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Extensions;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class WelcomeHandler : BaseHandler, IAbyssSignalListener<ClientReadyEvent>
{
    private readonly DirectoriesConfig _directoriesConfig;

    private List<string> _motd;

    private readonly IStringMessageService _stringMessageService;


    private readonly ITextTemplateService _textTemplateService;

    public WelcomeHandler(
        ILogger<WelcomeHandler> logger,
        DirectoriesConfig directoriesConfig,
        IStringMessageService stringMessageService,
        ITextTemplateService textTemplateService,
        IServiceProvider serviceProvider
    ) : base(logger, serviceProvider)
    {
        _directoriesConfig = directoriesConfig;
        _stringMessageService = stringMessageService;

        _textTemplateService = textTemplateService;

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

        var welcomeMessage = _stringMessageService.GetMessage(
            new RplWelcomeCommand().Code,
            session
        );

        var hostInfo = _stringMessageService.GetMessage(new RplYourHostCommand().Code, session);
        var createdInfo = _stringMessageService.GetMessage(new RplCreatedCommand().Code, session);

        SendIrcMessageAsync(
            signalEvent.Id,
            new RplWelcomeCommand(ServerData.Hostname, session.Nickname, welcomeMessage)
        );

        SendIrcMessageAsync(
            signalEvent.Id,
            new RplYourHostCommand(ServerData.Hostname, session.Nickname, hostInfo)
        );

        SendIrcMessageAsync(
            signalEvent.Id,
            new RplCreatedCommand(ServerData.Hostname, session.Nickname, createdInfo)
        );


        SendIrcMessageAsync(signalEvent.Id, ServerConfig.ToRplCommand(session.Nickname));

        SendIrcMessageAsync(signalEvent.Id, RplMotdStart.Create(ServerData.Hostname, session.Nickname));

        foreach (var line in _motd)
        {
            SendIrcMessageAsync(
                signalEvent.Id,
                new RplMotd(
                    ServerData.Hostname,
                    session.Nickname,
                    _textTemplateService.TranslateText(line, session)
                )
            );
        }

        SendIrcMessageAsync(signalEvent.Id, new RplEndOfMotd(ServerData.Hostname, session.Nickname));
    }
}
