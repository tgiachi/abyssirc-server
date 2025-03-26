using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class WelcomeHandler : BaseHandler, IAbyssSignalListener<ClientReadyEvent>
{
    private readonly AbyssIrcConfig _abyssIrcConfig;

    private readonly DirectoriesConfig _directoriesConfig;

    private List<string> _motd;

    private readonly IStringMessageService _stringMessageService;

    private readonly ISessionManagerService _sessionManagerService;

    private readonly ITextTemplateService _textTemplateService;

    public WelcomeHandler(
        ILogger<WelcomeHandler> logger,
        IAbyssSignalService signalService, AbyssIrcConfig abyssIrcConfig, DirectoriesConfig directoriesConfig,
        IStringMessageService stringMessageService, ISessionManagerService sessionManagerService,
        ITextTemplateService textTemplateService
    ) : base(logger, signalService, sessionManagerService)
    {
        _abyssIrcConfig = abyssIrcConfig;
        _directoriesConfig = directoriesConfig;
        _stringMessageService = stringMessageService;
        _sessionManagerService = sessionManagerService;
        _textTemplateService = textTemplateService;

        signalService.Subscribe(this);

        CheckMOTDFile();
    }


    private void CheckMOTDFile()
    {
        if (_abyssIrcConfig.Motd.Motd.StartsWith("file://"))
        {
            var motdFile = Path.Combine(_directoriesConfig.Root, _abyssIrcConfig.Motd.Motd.Replace("file://", ""));

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
            _motd = _abyssIrcConfig.Motd.Motd.Split('\n').ToList();
        }
    }


    public async Task OnEventAsync(ClientReadyEvent signalEvent)
    {
        var session = _sessionManagerService.GetSession(signalEvent.Id);

        var welcomeMessage = _stringMessageService.GetMessage(
            new RplWelcomeCommand().Code,
            session
        );

        var hostInfo = _stringMessageService.GetMessage(new RplYourHostCommand().Code, session);
        var createdInfo = _stringMessageService.GetMessage(new RplCreatedCommand().Code, session);

        SendIrcMessageAsync(
            signalEvent.Id,
            new RplWelcomeCommand(_abyssIrcConfig.Network.Host, session.Nickname, welcomeMessage)
        );

        SendIrcMessageAsync(
            signalEvent.Id,
            new RplYourHostCommand(_abyssIrcConfig.Network.Host, session.Nickname, hostInfo)
        );

        SendIrcMessageAsync(
            signalEvent.Id,
            new RplCreatedCommand(_abyssIrcConfig.Network.Host, session.Nickname, createdInfo)
        );

        foreach (var isupportCommand in CreateISupportCommand(session.Username))
        {
            SendIrcMessageAsync(signalEvent.Id, isupportCommand);
        }

        SendIrcMessageAsync(signalEvent.Id, new RplMotdStart(_abyssIrcConfig.Network.Host, session.Nickname));
        foreach (var line in _motd)
        {
            SendIrcMessageAsync(
                signalEvent.Id,
                new RplMotd(
                    _abyssIrcConfig.Network.Host,
                    session.Nickname,
                    _textTemplateService.TranslateText(line, session)
                )
            );
        }

        SendIrcMessageAsync(signalEvent.Id, new RplEndOfMotd(_abyssIrcConfig.Network.Host, session.Nickname));
    }

    private List<IIrcCommand> CreateISupportCommand(string username)
    {
        // First ISUPPORT message - general server capabilities
        var isupport1 = RplISupport.Create(
            _abyssIrcConfig.Network.Host,
            username,
            "WHOX",
            "WALLCHOPS",
            "WALLVOICES",
            "USERIP",
            "CPRIVMSG",
            "CNOTICE",
            $"SILENCE=15",
            $"MODES=6",
            $"MAXCHANNELS={_abyssIrcConfig.Limits.MaxChannelsPerUser}",
            $"MAXBANS={_abyssIrcConfig.Limits.MaxBansPerChannel}",
            $"NICKLEN={_abyssIrcConfig.Limits.MaxNickLength}"
        );

        // Second ISUPPORT message - channel and formatting related capabilities
        var isupport2 = RplISupport.Create(
            _abyssIrcConfig.Network.Host,
            username,
            $"MAXNICKLEN={_abyssIrcConfig.Limits.MaxNickLength}",
            $"TOPICLEN={_abyssIrcConfig.Limits.MaxTopicLength}",
            $"AWAYLEN=160",
            $"KICKLEN=250",
            $"CHANNELLEN={_abyssIrcConfig.Limits.MaxChannelNameLength}",
            $"MAXCHANNELLEN={_abyssIrcConfig.Limits.MaxChannelNameLength}",
            "CHANTYPES=#&",
            "PREFIX=(ov)@+",
            "STATUSMSG=@+",
            $"CHANMODES={_abyssIrcConfig.Limits.ChannelModes}",
            "CASEMAPPING=rfc1459",
            $"NETWORK={_abyssIrcConfig.Admin.NetworkName}"
        );

        return [isupport1, isupport2];
    }
}
