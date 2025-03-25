using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Data.Directories;
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

    private readonly ITextTemplateService _textTemplateService;

    private readonly ISessionManagerService _sessionManagerService;

    public WelcomeHandler(
        ILogger<WelcomeHandler> logger,
        IAbyssSignalService signalService, AbyssIrcConfig abyssIrcConfig, DirectoriesConfig directoriesConfig,
        ITextTemplateService textTemplateService, ISessionManagerService sessionManagerService
    ) : base(logger, signalService)
    {
        _abyssIrcConfig = abyssIrcConfig;
        _directoriesConfig = directoriesConfig;
        _textTemplateService = textTemplateService;
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
    }
}
