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

    private static HostApplicationBuilder _hostBuilder;

    private static DirectoriesConfig _directoriesConfig;

    private static AbyssIrcConfig _config;


    private static IHost _app;


    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AbyssIrcOptions))]
    static async Task Main(string[] args)
    {
        AbyssIrcOptions options = null;

        var restartFlag = Environment.GetEnvironmentVariable("ABYSS_RESTART");
        var restartReason = Environment.GetEnvironmentVariable("ABYSS_RESTARTREASON");

        if (!string.IsNullOrEmpty(restartFlag) && restartFlag.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Server restarted. Reason: {restartReason ?? "Unspecified"}");

            Environment.SetEnvironmentVariable("ABYSS_RESTART", null);
            Environment.SetEnvironmentVariable("ABYSS_RESTARTREASON", null);
        }

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


        if (Environment.GetEnvironmentVariable("ABYSS_ENABLE_DEBUG") != null)
        {
            options.EnableDebug = true;
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
        _config = await LoadConfigAsync(_directoriesConfig.Root, options.ConfigFile);

        _hostBuilder.Services.AddSingleton(_config.ToServerData());

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

            // Additional log file for specific logs (e.g., TCP/Network)
            loggingConfig.WriteTo.Logger(
                lc => lc
                    .MinimumLevel.Debug()
                    .WriteTo.File(
                        formatter: new CompactJsonFormatter(),
                        path: Path.Combine(_directoriesConfig[DirectoryType.Logs], "network_debug_.log"),
                        rollingInterval: RollingInterval.Day
                    )
                    // Filter to include only logs from specific namespaces or with specific properties
                    .Filter.ByIncludingOnly(
                        e =>
                            e.Properties.ContainsKey("SourceContext") &&
                            e.Properties["SourceContext"].ToString().Contains("Tcp") ||
                            e.Properties.ContainsKey("SourceContext").ToString().Contains("TcpService")
                    )
            );
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
            .RegisterIrcCommandListener<NickUserHandler>(new IsonCommand())
            .RegisterIrcCommandListener<NickUserHandler>(new ModeCommand())
            .RegisterIrcCommandListener<PingPongHandler>(new PingCommand())
            .RegisterIrcCommandListener<PingPongHandler>(new PongCommand())
            .RegisterIrcCommandListener<ServerCommandsListener>(new RestartCommand())
            .RegisterIrcCommandListener<PassHandler>(new PassCommand())
            .RegisterIrcCommandListener<PrivMsgHandler>(new PrivMsgCommand())

            //Channel management
            .RegisterIrcCommandListener<ChannelsHandler>(new PrivMsgCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new JoinCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new PartCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new ModeCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new ListCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new NamesCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new TopicCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new PartCommand())
            ;


        _hostBuilder.Services
            .RegisterIrcCommand(new RplMyInfo())
            .RegisterIrcCommand(new RplWelcome())
            .RegisterIrcCommand(new RplYourHost())
            .RegisterIrcCommand(new CapCommand())
            .RegisterIrcCommand(new NickCommand())
            .RegisterIrcCommand(new UserCommand())
            .RegisterIrcCommand(new NoticeCommand())
            .RegisterIrcCommand(new PingCommand())
            .RegisterIrcCommand(new PongCommand())
            .RegisterIrcCommand(new PrivMsgCommand())
            .RegisterIrcCommand(new ModeCommand())
            .RegisterIrcCommand(new QuitCommand())
            .RegisterIrcCommand(new IsonCommand())
            .RegisterIrcCommand(new UserhostCommand())
            .RegisterIrcCommand(new PassCommand())
            .RegisterIrcCommand(new ListCommand())
            .RegisterIrcCommand(new AdminCommand())
            .RegisterIrcCommand(new InfoCommand())
            .RegisterIrcCommand(new JoinCommand())
            .RegisterIrcCommand(new PartCommand())
            .RegisterIrcCommand(new ListCommand())
            .RegisterIrcCommand(new RestartCommand())
            .RegisterIrcCommand(new NamesCommand())
            .RegisterIrcCommand(new TopicCommand())
            ;


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
            .RegisterAutoStartService<IChannelManagerService, ChannelManagerService>()
            .RegisterAutoStartService<ITcpService, TcpService>()
            ;


        _hostBuilder.Services
            .RegisterScriptModule<LoggerModule>()
            .RegisterScriptModule<EventsModule>()
            .RegisterScriptModule<SchedulerModule>()
            .RegisterScriptModule<IrcManagerModule>()
            .RegisterScriptModule<VariableModule>()
            ;


        _hostBuilder.Services.AddHostedService<AbyssIrcHostService>();

        _hostBuilder.Logging.ClearProviders().AddSerilog();

        _app = _hostBuilder.Build();


        await _app.RunAsync();
    }

    private static async Task<AbyssIrcConfig> LoadConfigAsync(string rootDirectory, string configFileName)
    {
        var configFile = Path.Combine(rootDirectory, configFileName);

        if (!File.Exists(configFile))
        {
            Log.Warning("Configuration file not found. Creating default configuration file...");

            var config = new AbyssIrcConfig();

            await File.WriteAllTextAsync(configFile, config.ToYaml());
        }

        Log.Logger.Information("Loading configuration file...");

        return (await File.ReadAllTextAsync(configFile)).FromYaml<AbyssIrcConfig>();
    }

    private static void ShowHeader()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "AbyssIrc.Server.Assets.header.txt";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var version = assembly.GetName().Version;

        Console.WriteLine(reader.ReadToEnd());
        Console.WriteLine($"  >> Version: {version}");
    }
}
