using System.Net;
using AbyssIrc.Core.Interfaces.Services;
using AbyssIrc.Core.Json;
using AbyssIrc.Server.Core.Data.Config;
using AbyssIrc.Server.Core.Data.Config.Sections;
using AbyssIrc.Server.Core.Data.Internal.Services;
using AbyssIrc.Server.Core.Data.Options;
using AbyssIrc.Server.Core.Directories;
using AbyssIrc.Server.Core.Extensions.Loggers;
using AbyssIrc.Server.Core.Interfaces.Services;
using AbyssIrc.Server.Core.Types.Directories;
using DryIoc;
using Serilog;

namespace AbyssIrc.Server.Core.Bootstrap;

public class AbyssIrcBoostrap
{
    private readonly IContainer _container;

    private readonly AbyssIrcOptions _options;

    private readonly CancellationTokenSource _cancellationTokenSource;

    public delegate IContainer RegisterHandler(IContainer container);

    public delegate void NetworkServiceHandler(INetworkService service);

    public event RegisterHandler OnRegisterServices;

    public event NetworkServiceHandler OnNetworkServices;

    private DirectoriesConfig _directoriesConfig;

    private AbyssIrcServerConfig _serverConfig;


    public AbyssIrcBoostrap(AbyssIrcOptions options, CancellationToken cancellationToken)
    {
        _options = options;
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _container = new Container(rules => rules
            .WithoutInterpretationForTheFirstResolution()
            .WithFactorySelector(Rules.SelectLastRegisteredFactory())
            .With(FactoryMethod.ConstructorWithResolvableArguments)
            .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace)
        );
    }

    public void Init()
    {
        InitDirectories();
        _serverConfig = InitializeConfig(Path.Combine(_directoriesConfig.Root, _options.Config));
        _container.RegisterInstance(_serverConfig);

        InitializeLogger();
        OnRegisterServices?.Invoke(_container);
        OnNetworkServices?.Invoke(_container.Resolve<INetworkService>());
    }

    private void InitDirectories()
    {
        _directoriesConfig = new DirectoriesConfig(_options.RootDirectory, Enum.GetNames<DirectoryType>());

        _container.RegisterInstance(_directoriesConfig);

        Console.WriteLine($"Root directory: " + _options.RootDirectory);
    }

    private void InitializeLogger()
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(_options.LogLevel.ToSerilogLogLevel())
            .Enrich.FromLogContext();

        if (_options.LogToConsole)
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Console();
        }

        if (_options.LogToFile)
        {
            loggerConfiguration.WriteTo.File(
                Path.Combine(_directoriesConfig[DirectoryType.Logs], $"abyssirc_server_.log"),
                rollingInterval: RollingInterval.Day
            );
        }


        Log.Logger = loggerConfiguration.CreateLogger();

        Log.Logger.Information("AbyssIrc logger initialized.");
    }


    private AbyssIrcServerConfig InitializeConfig(string configName)
    {
        Console.WriteLine($"Loading config: {configName}");
        var config = new AbyssIrcServerConfig();

        var configPath = Path.Combine(_directoriesConfig.Root, configName);

        if (!File.Exists(configPath))
        {
            JsonUtils.SerializeToFile(config, configPath);
        }

        config = JsonUtils.DeserializeFromFile<AbyssIrcServerConfig>(
            configPath
        );

        JsonUtils.SerializeToFile(config, configPath);

        if (_options.SecurePorts.Length > 0)
        {
            config.Network.Binds.Add(
                new NetworkBindConfig()
                {
                    Host = IPAddress.Any,
                    Ports = _options.SecurePorts,
                    UseSsl = true
                }
            );
        }

        if (_options.NonSecurePorts.Length > 0)
        {
            config.Network.Binds.Add(
                new NetworkBindConfig()
                {
                    Host = IPAddress.Any,
                    Ports = _options.NonSecurePorts,
                    UseSsl = false
                }
            );
        }

        return config;
    }


    public async Task StartAsync()
    {
        await StartStopServiceAsync(true);

        try
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    public async Task StopAsync()
    {
        await StartStopServiceAsync(false);
        Console.WriteLine("AbyssIrc Server Stopped");
    }

    private async Task StartStopServiceAsync(bool isStart)
    {
        var servicesDef = _container.Resolve<List<ServiceDefinitionObject>>().OrderBy(s => s.Priority).ToList();


        foreach (var serviceDef in servicesDef)
        {
            _container.Resolve(serviceDef.ServiceType);
            Log.Logger.Debug("Ctor service: {ServiceType}", serviceDef.ImplementationType.Name);
        }

        foreach (var serviceDef in servicesDef)
        {
            try
            {
                var serviceInstance = _container.Resolve(serviceDef.ServiceType);
                if (serviceInstance is IAbyssStarStopService startableService)
                {
                    if (isStart)
                    {
                        Log.Logger.Debug("Starting service: {ServiceType}", serviceDef.ImplementationType.Name);
                        await startableService.StartAsync();
                    }
                    else
                    {
                        Log.Logger.Debug("Stopping service: {ServiceType}", serviceDef.ImplementationType.Name);
                        await startableService.StopAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(
                    ex,
                    "Error while {Action} service: {ServiceType}",
                    isStart ? "starting" : "stopping",
                    serviceDef.ImplementationType.Name
                );
                throw new InvalidOperationException(
                    $"Failed to {(isStart ? "start" : "stop")} service: {serviceDef.ImplementationType.Name}",
                    ex
                );
            }
        }

        Log.Logger.Information("AbyssIRC services {Action} successfully.", isStart ? "started" : "stopped");
    }
}
