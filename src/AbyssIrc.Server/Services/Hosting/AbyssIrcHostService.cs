using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Services.Hosting;

public class AbyssIrcHostService : IHostedService
{
    private readonly ILogger _logger;

    private readonly IAbyssSignalService _signalService;
    private readonly IIrcCommandParser _ircCommandParser;
    private readonly IIrcManagerService _ircManagerService;
    private readonly ISessionManagerService _sessionManagerService;
    private readonly ITcpService _tcpService;
    private readonly IServiceProvider _serviceProvider;

    public AbyssIrcHostService(
        ILogger<AbyssIrcHostService> logger, IAbyssSignalService signalService,
        IIrcCommandParser ircCommandParser, IIrcManagerService ircManagerService,
        ISessionManagerService sessionManagerService, ITcpService tcpService, IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _signalService = signalService;
        _ircCommandParser = ircCommandParser;
        _ircManagerService = ircManagerService;
        _sessionManagerService = sessionManagerService;
        _tcpService = tcpService;
        _serviceProvider = serviceProvider;

        RegisterCommands();
        RegisterListeners();
    }

    private void RegisterListeners()
    {
        _ircManagerService.RegisterListener(new QuitCommand().Code, _serviceProvider.GetService<QuitMessageHandler>());

        _serviceProvider.GetService<ConnectionHandler>();
    }

    private void RegisterCommands()
    {
        _ircCommandParser.RegisterCommand(new RplMyInfoCommand());
        _ircCommandParser.RegisterCommand(new RplWelcomeCommand());
        _ircCommandParser.RegisterCommand(new RplYourHostCommand());

        _ircCommandParser.RegisterCommand(new CapCommand());
        _ircCommandParser.RegisterCommand(new NickCommand());
        _ircCommandParser.RegisterCommand(new UserCommand());

        _ircCommandParser.RegisterCommand(new NoticeCommand());

        _ircCommandParser.RegisterCommand(new QuitCommand());
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _tcpService.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _tcpService.StopAsync();
    }
}
