using System.Diagnostics.CodeAnalysis;
using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Core.Types;
using AbyssIrc.Server.Data;
using AbyssIrc.Server.ServiceProvider;
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
            await Task.Delay(1000);


            await Task.Delay(Timeout.Infinite, _cancellationToken.Token);
        }
        catch (OperationCanceledException)
        {
            Log.Information("Request to shutting down...");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred");
        }
    }
}
