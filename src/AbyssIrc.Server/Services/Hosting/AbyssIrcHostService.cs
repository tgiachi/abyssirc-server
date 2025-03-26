using System.Diagnostics;
using System.Reflection;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Events.Core;
using AbyssIrc.Core.Interfaces.Services;
using AbyssIrc.Network.Data.Internal;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Server.Data.Internal.ServiceCollection;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services.Server;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Signals.Interfaces.Services;
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
    private readonly IAbyssSignalService _signalService;

    private readonly List<IrcCommandListenerDefinitionData> _ircHandlers;

    private readonly List<AutoStartDefinitionData> _autoStartServices;
    private readonly List<IrcCommandDefinitionData> _ircCommands;

    public AbyssIrcHostService(
        ILogger<AbyssIrcHostService> logger,
        ITcpService tcpService, IServiceProvider serviceProvider,
        AbyssIrcConfig abyssIrcConfig,
        IAbyssSignalService signalService,
        List<IrcCommandListenerDefinitionData> ircHandlers,
        List<IrcCommandDefinitionData> ircCommands,
        List<AutoStartDefinitionData> autoStartServices
    )
    {
        _logger = logger;


        _tcpService = tcpService;
        _serviceProvider = serviceProvider;

        _abyssIrcConfig = abyssIrcConfig;
        _ircHandlers = ircHandlers;
        _ircCommands = ircCommands;
        _autoStartServices = autoStartServices;
        _signalService = signalService;

        RegisterCommands();
        RegisterListeners();
        RegisterVariables();

        InitServices();
    }

    private void InitServices()
    {
        foreach (var service in _autoStartServices)
        {
            _serviceProvider.GetRequiredService(service.InterfaceType);
        }
    }

    private async Task StartServicesAsync()
    {
        foreach (var service in _autoStartServices)
        {
            var serviceInstance = _serviceProvider.GetRequiredService(service.InterfaceType) as IAbyssStarStopService;

            if (serviceInstance == null)
            {
                continue;
            }

            _logger.LogDebug("Starting {Service}", service.InterfaceType.Name);
            await serviceInstance.StartAsync();
        }
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

        foreach (var handler in _ircHandlers)
        {
            ircManagerService.RegisterListener(
                handler.Command,
                _serviceProvider.GetService(handler.HandlerType) as IIrcMessageListener
            );
        }
    }

    private void RegisterCommands()
    {
        var ircCommandParser = _serviceProvider.GetRequiredService<IIrcCommandParser>();

        foreach (var cmd in _ircCommands)
        {
            ircCommandParser.RegisterCommand(cmd.Command);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartServicesAsync();
        await _signalService.PublishAsync(new ServerReadyEvent(), cancellationToken);
        await _tcpService.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _tcpService.StopAsync();
    }
}
