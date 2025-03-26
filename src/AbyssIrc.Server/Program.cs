using System.Diagnostics.CodeAnalysis;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Core.Types;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Network.Services;
using AbyssIrc.Server.Data.Options;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Interfaces.Services.Server;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Listeners;
using AbyssIrc.Server.Services;
using AbyssIrc.Server.Services.Hosting;
using AbyssIrc.Signals.Data.Configs;
using AbyssIrc.Signals.Interfaces.Services;
using AbyssIrc.Signals.Services;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

namespace AbyssIrc.Server;

class Program
{
    private static readonly CancellationTokenSource _cancellationToken = new();

    private static HostApplicationBuilder _hostBuilder;

    private static DirectoriesConfig _directoriesConfig;

    private static AbyssIrcConfig _config;


    private static IHost _app;


    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AbyssIrcOptions))]
    static async Task Main(string[] args)
    {
        AbyssIrcOptions options = null;

        Parser.Default.ParseArguments<AbyssIrcOptions>(args)
            .WithParsed(
                ircOptions => { options = ircOptions; }
            )
            .WithNotParsed(
                (e) =>
                {
                    // show help message

                    Environment.Exit(1);
                    return;
                }
            );


        if (Environment.GetEnvironmentVariable("ABYSS_ROOT_DIRECTORY") != null)
        {
            options.RootDirectory = Environment.GetEnvironmentVariable("ABYSS_ROOT_DIRECTORY");
        }


        _hostBuilder = Host.CreateApplicationBuilder(args);

        if (string.IsNullOrWhiteSpace(options?.RootDirectory))
        {
            options.RootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "abyss");
        }

        _directoriesConfig = new DirectoriesConfig(options.RootDirectory);

        _hostBuilder.Services.AddSingleton(_directoriesConfig);

        _hostBuilder.Services.AddSingleton(
            new AbyssIrcSignalConfig()
            {
                DispatchTasks = Environment.ProcessorCount,
            }
        );

        var configFile = Path.Combine(_directoriesConfig.Root, options.ConfigFile);

        if (!File.Exists(configFile))
        {
            Log.Warning("Configuration file not found. Creating default configuration file...");

            var config = new AbyssIrcConfig();

            await File.WriteAllTextAsync(configFile, config.ToJsonAot());
        }

        Log.Logger.Information("Loading configuration file...");

        _config = (await File.ReadAllTextAsync(configFile)).FromJsonAot<AbyssIrcConfig>();

        _hostBuilder.Services.AddSingleton(_config);

        var loggingConfig = new LoggerConfiguration()
            .WriteTo.File(
                formatter: new CompactJsonFormatter(),
                Path.Combine(_directoriesConfig[DirectoryType.Logs], "abyss_server_.log"),
                rollingInterval: RollingInterval.Day
            )
            .WriteTo.Console();

        if (options.EnableDebug)
        {
            loggingConfig.MinimumLevel.Debug();
        }
        else
        {
            loggingConfig.MinimumLevel.Information();
        }

        Log.Logger = loggingConfig.CreateLogger();

        _hostBuilder.Services
            .AddSingleton<IAbyssSignalService, AbyssSignalService>()
            .AddSingleton<IIrcCommandParser, IrcCommandParser>()
            .AddSingleton<IIrcManagerService, IrcManagerService>()
            .AddSingleton<ISessionManagerService, SessionManagerService>()
            .AddSingleton<ITextTemplateService, TextTemplateService>()
            .AddSingleton<IStringMessageService, StringMessageService>()
            .AddSingleton<ISchedulerSystemService, SchedulerSystemService>()
            .AddSingleton<ITcpService, TcpService>()
            ;

        _hostBuilder.Services
            .AddSingleton<ConnectionHandler>()
            .AddSingleton<QuitMessageHandler>()
            .AddSingleton<WelcomeHandler>()
            .AddSingleton<NickUserHandler>()
            .AddSingleton<PingPongHandler>()
            ;


        _hostBuilder.Services.AddHostedService<AbyssIrcHostService>();

        _hostBuilder.Logging.ClearProviders().AddSerilog();

        _app = _hostBuilder.Build();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Log.Information("Shutting down...");
            Log.CloseAndFlush();

            _cancellationToken.Cancel();
        };

        try
        {
            Log.Information("Starting AbyssIrc Server...");
            // _serverProvider.GetService<IAbyssIrcSignalEmitterService>();
            //
            // var commandParser = _serverProvider.GetService<IIrcCommandParser>();
            //
            // commandParser.RegisterCommand(new RplCreatedCommand());
            // commandParser.RegisterCommand(new RplMyInfoCommand());
            // commandParser.RegisterCommand(new RplWelcomeCommand());
            // commandParser.RegisterCommand(new RplYourHostCommand());
            //
            // commandParser.RegisterCommand(new CapCommand());
            // commandParser.RegisterCommand(new NickCommand());
            // commandParser.RegisterCommand(new UserCommand());
            //
            // commandParser.RegisterCommand(new NoticeCommand());
            //
            // commandParser.RegisterCommand(new QuitCommand());
            //
            //
            // var ircManagerService = _serverProvider.GetService<IIrcManagerService>();
            //
            // ircManagerService.RegisterListener(new QuitCommand().Code, _serverProvider.GetService<QuitMessageHandler>());
            //
            // _serverProvider.GetService<ISessionManagerService>();
            // _serverProvider.GetService<ConnectionHandler>();
            //
            //
            // _serverProvider.GetService<ISessionManagerService>();
            //
            //
            // await _serverProvider.GetService<ITcpService>().StartAsync();

            await _app.RunAsync(_cancellationToken.Token);

            await Task.Delay(Timeout.Infinite, _cancellationToken.Token);
        }
        catch (OperationCanceledException)
        {
            Log.Information("Request to shutting down...");
            //  await _serverProvider.GetService<ITcpService>().StopAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred");
        }
    }
}
