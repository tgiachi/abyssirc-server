using System.Diagnostics.CodeAnalysis;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Core.Types;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Server.Data;
using AbyssIrc.Server.Interfaces;
using AbyssIrc.Server.ServiceProvider;
using AbyssIrc.Signals.Data.Configs;
using AbyssIrc.Signals.Interfaces.Services;
using CommandLine;
using Serilog;
using Serilog.Formatting.Json;

namespace AbyssIrc.Server;

class Program
{
    private static readonly CancellationTokenSource _cancellationToken = new();
    private static readonly AbyssIrcServerProvider _serverProvider = new();

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AbyssIrcOptions))]
    static async Task Main(string[] args)
    {
        var options = Parser.Default.ParseArguments<AbyssIrcOptions>(args).Value;


        if (string.IsNullOrWhiteSpace(options?.RootDirectory))
        {
            options.RootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "abyss");
        }

        _serverProvider.DirectoriesConfig = new DirectoriesConfig(options.RootDirectory);
        _serverProvider.AbyssIrcSignalConfig = new AbyssIrcSignalConfig()
        {
            DispatchTasks = Environment.ProcessorCount,
        };

        var configFile = Path.Combine(_serverProvider.DirectoriesConfig.Root, options.ConfigFile);

        if (!File.Exists(configFile))
        {
            Log.Warning("Configuration file not found. Creating default configuration file...");

            var config = new AbyssIrcConfig();

            await File.WriteAllTextAsync(configFile, config.ToJsonAot());
        }

        Log.Logger.Information("Loading configuration file...");
        _serverProvider.AbyssIrcConfig = (await File.ReadAllTextAsync(configFile)).FromJsonAot<AbyssIrcConfig>();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(
                formatter: new JsonFormatter(),
                Path.Combine(_serverProvider.DirectoriesConfig[DirectoryType.Logs], "abyss_server_.log"),
                rollingInterval: RollingInterval.Day
            )
            .WriteTo.Console()
            .CreateLogger();

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
            _serverProvider.GetService<IAbyssIrcSignalEmitterService>();

            var commandParser = _serverProvider.GetService<IIrcCommandParser>();

            commandParser.RegisterCommand(new RplCreatedCommand());
            commandParser.RegisterCommand(new RplMyInfoCommand());
            commandParser.RegisterCommand(new RplWelcomeCommand());
            commandParser.RegisterCommand(new RplYourHostCommand());


            await _serverProvider.GetService<ITcpService>().StartAsync();


            await Task.Delay(Timeout.Infinite, _cancellationToken.Token);
        }
        catch (OperationCanceledException)
        {
            Log.Information("Request to shutting down...");
            await _serverProvider.GetService<ITcpService>().StopAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred");
        }
    }
}
