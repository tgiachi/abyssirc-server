using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Core.Types;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Network.Services;
using AbyssIrc.Server.Data.Options;
using AbyssIrc.Server.Extensions;
using AbyssIrc.Server.Interfaces.Services.Server;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Listeners;
using AbyssIrc.Server.Modules.Scripts;
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


namespace AbyssIrc.Server;

class Program
{
    //https://github.com/ValwareIRC/valware-unrealircd-mods/blob/main/auto-away/auto-away.c
    //https://modern.ircdocs.horse/#privmsg-message
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


        if (options.ShowHeader)
        {
            ShowHeader();
        }

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

        if (!string.IsNullOrWhiteSpace(options.HostName))
        {
            Log.Logger.Information("Override hostname to :{HostName}", options.HostName);

            _config.Network.Host = options.HostName;
        }


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
            .RegisterIrcCommandListener<QuitMessageHandler>(new QuitCommand())
            .RegisterIrcCommandListener<NickUserHandler>(new UserCommand())
            .RegisterIrcCommandListener<NickUserHandler>(new NickCommand())
            .RegisterIrcCommandListener<PingPongHandler>(new PingCommand())
            .RegisterIrcCommandListener<PingPongHandler>(new PongCommand())
            .RegisterIrcCommandListener<PrivMsgHandler>(new PrivMsgCommand());


        _hostBuilder.Services
            .RegisterIrcCommand(new RplMyInfoCommand())
            .RegisterIrcCommand(new RplWelcomeCommand())
            .RegisterIrcCommand(new RplYourHostCommand())
            .RegisterIrcCommand(new CapCommand())
            .RegisterIrcCommand(new NickCommand())
            .RegisterIrcCommand(new UserCommand())
            .RegisterIrcCommand(new NoticeCommand())
            .RegisterIrcCommand(new PingCommand())
            .RegisterIrcCommand(new PongCommand())
            .RegisterIrcCommand(new PrivMsgCommand())
            .RegisterIrcCommand(new ModeCommand())
            .RegisterIrcCommand(new QuitCommand());


        // Register handlers

        _hostBuilder.Services
            .RegisterIrcHandler<ConnectionHandler>()
            .RegisterIrcHandler<NickUserHandler>()
            .RegisterIrcHandler<PingPongHandler>()
            .RegisterIrcHandler<PrivMsgHandler>()
            .RegisterIrcHandler<QuitMessageHandler>()
            .RegisterIrcHandler<WelcomeHandler>()
            ;


        _hostBuilder.Services
            .RegisterAutoStartService<IAbyssSignalService, AbyssSignalService>()
            .RegisterAutoStartService<IIrcCommandParser, IrcCommandParser>()
            .RegisterAutoStartService<IIrcManagerService, IrcManagerService>()
            .RegisterAutoStartService<ISessionManagerService, SessionManagerService>()
            .RegisterAutoStartService<ITextTemplateService, TextTemplateService>()
            .RegisterAutoStartService<IStringMessageService, StringMessageService>()
            .RegisterAutoStartService<ISchedulerSystemService, SchedulerSystemService>()
            .RegisterAutoStartService<IScriptEngineService, ScriptEngineService>()
            .RegisterAutoStartService<IEventDispatcherService, EventDispatcherService>()
            .RegisterAutoStartService<ITcpService, TcpService>()
            ;


        _hostBuilder.Services
            .RegisterScriptModule<LoggerModule>()
            .RegisterScriptModule<EventsModule>()
            .RegisterScriptModule<IrcManagerModule>()
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

    private static void ShowHeader()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "AbyssIrc.Server.Assets.header.txt";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var version = assembly.GetName().Version;

        Console.WriteLine(reader.ReadToEnd());
        Console.WriteLine($"Version: {version}");
    }
}
