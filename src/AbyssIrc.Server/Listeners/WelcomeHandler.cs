using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class WelcomeHandler : BaseHandler, IAbyssSignalListener<ClientReadyEvent>
{
    private readonly AbyssIrcConfig _abyssIrcConfig;

    private readonly DirectoriesConfig _directoriesConfig;

    private string _motd;

    private readonly IStringMessageService _stringMessageService;

    private readonly ISessionManagerService _sessionManagerService;

    public WelcomeHandler(
        ILogger<WelcomeHandler> logger,
        IAbyssSignalService signalService, AbyssIrcConfig abyssIrcConfig, DirectoriesConfig directoriesConfig,
        IStringMessageService stringMessageService, ISessionManagerService sessionManagerService
    ) : base(logger, signalService)
    {
        _abyssIrcConfig = abyssIrcConfig;
        _directoriesConfig = directoriesConfig;
        _stringMessageService = stringMessageService;
        _sessionManagerService = sessionManagerService;
        signalService.Subscribe(this);

        CheckMOTDFile();
    }


    private void CheckMOTDFile()
    {
        var motdFile = Path.Combine(_directoriesConfig.Root, _abyssIrcConfig.Motd.MotdFile);

        if (!File.Exists(motdFile))
        {
            Logger.LogError("MOTD file not found: {motdFile}", motdFile);

            throw new FileNotFoundException($"MOTD file not found: {motdFile}");
        }

        Logger.LogInformation("MOTD file found: {motdFile}", motdFile);

        _motd = File.ReadAllText(motdFile);
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
            new RplWelcomeCommand(_abyssIrcConfig.Network.Host, session.Username, welcomeMessage)
        );

        SendIrcMessageAsync(
            signalEvent.Id,
            new RplYourHostCommand(_abyssIrcConfig.Network.Host, session.Username, hostInfo)
        );

        SendIrcMessageAsync(
            signalEvent.Id,
            new RplCreatedCommand(_abyssIrcConfig.Network.Host, session.Username, createdInfo)
        );

        foreach (var isupportCommand in CreateISupportCommand(session.Username))
        {
            SendIrcMessageAsync(signalEvent.Id, isupportCommand);
        }
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
//<- :irc.abyssirc.com 005 tomm MAXNICKLEN=30 TOPICLEN=390 AWAYLEN=160 KICKLEN=250 CHANNELLEN=50 MAXCHANNELLEN=50 CHANTYPES=#& PREFIX=(ov)@+ STATUSMSG=@+ CHANMODES=bklmntsiIpK CASEMAPPING=rfc1459 NETWORK=AbyssIRC :are supported by this server
//<- :atw.hu.quakenet.org 005 fasdf MAXNICKLEN=15 TOPICLEN=250 AWAYLEN=160 KICKLEN=250 CHANNELLEN=200 MAXCHANNELLEN=200 CHANTYPES=#& PREFIX=(ov)@+ STATUSMSG=@+ CHANMODES=b,k,l,imnpstrDducCNMT CASEMAPPING=rfc1459 NETWORK=QuakeNet :are supported by this server
