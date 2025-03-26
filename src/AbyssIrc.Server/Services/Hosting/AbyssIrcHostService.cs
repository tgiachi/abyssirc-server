using System.Diagnostics;
using System.Reflection;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Listeners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Services.Hosting;

public class AbyssIrcHostService : IHostedService
{
    private readonly ILogger _logger;


    private readonly ITcpService _tcpService;
    private readonly AbyssIrcConfig _abyssIrcConfig;
    private readonly IServiceProvider _serviceProvider;

    public AbyssIrcHostService(
        ILogger<AbyssIrcHostService> logger,
        ITcpService tcpService, IServiceProvider serviceProvider,
        AbyssIrcConfig abyssIrcConfig
    )
    {
        _logger = logger;


        _tcpService = tcpService;
        _serviceProvider = serviceProvider;

        _abyssIrcConfig = abyssIrcConfig;

        RegisterCommands();
        RegisterListeners();
        RegisterVariables();

        InitServices();
    }

    private void InitServices()
    {
        _serviceProvider.GetRequiredService<ISessionManagerService>();
        _serviceProvider.GetRequiredService<IStringMessageService>();
    }

    private void RegisterVariables()
    {
        var textTemplateService = _serviceProvider.GetRequiredService<ITextTemplateService>();
        textTemplateService.AddVariable("hostname", _abyssIrcConfig.Network.Host);
        textTemplateService.AddVariable("version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
        textTemplateService.AddVariable("created", DateTime.Now.ToString("F"));
        textTemplateService.AddVariableBuilder(
            "uptime",
            () => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString()
        );

        textTemplateService.AddVariable("admin_email", _abyssIrcConfig.Admin.AdminEmail);
        textTemplateService.AddVariable("network_name", _abyssIrcConfig.Admin.NetworkName);
    }

    private void RegisterListeners()
    {
        var ircManagerService = _serviceProvider.GetRequiredService<IIrcManagerService>();
        ircManagerService.RegisterListener(new QuitCommand().Code, _serviceProvider.GetService<QuitMessageHandler>());
        ircManagerService.RegisterListener(new UserCommand().Code, _serviceProvider.GetService<NickUserHandler>());
        ircManagerService.RegisterListener(new NickCommand().Code, _serviceProvider.GetService<NickUserHandler>());


        _serviceProvider.GetService<ConnectionHandler>();
        _serviceProvider.GetService<WelcomeHandler>();
    }

    private void RegisterCommands()
    {
        var ircCommandParser = _serviceProvider.GetRequiredService<IIrcCommandParser>();
        ircCommandParser.RegisterCommand(new RplMyInfoCommand());
        ircCommandParser.RegisterCommand(new RplWelcomeCommand());
        ircCommandParser.RegisterCommand(new RplYourHostCommand());

        ircCommandParser.RegisterCommand(new CapCommand());
        ircCommandParser.RegisterCommand(new NickCommand());
        ircCommandParser.RegisterCommand(new UserCommand());

        ircCommandParser.RegisterCommand(new NoticeCommand());

        ircCommandParser.RegisterCommand(new QuitCommand());
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
