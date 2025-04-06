using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Core.Utils;
using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Protocol.Messages.Interfaces.Parser;
using AbyssIrc.Protocol.Messages.Services;
using AbyssIrc.Server.Core.Data.Configs;
using AbyssIrc.Server.Core.Data.Directories;
using AbyssIrc.Server.Core.Extensions;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Core.Types;
using AbyssIrc.Server.Data.Options;
using AbyssIrc.Server.Extensions;
using AbyssIrc.Server.Listeners;
using AbyssIrc.Server.Middleware;
using AbyssIrc.Server.Modules.Scripts;
using AbyssIrc.Server.Plugins.Core;
using AbyssIrc.Server.Routes;
using AbyssIrc.Server.Services;
using AbyssIrc.Server.Services.Hosting;
using AbyssIrc.Signals.Data.Configs;
using AbyssIrc.Signals.Interfaces.Services;
using AbyssIrc.Signals.Services;
using CommandLine;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;


namespace AbyssIrc.Server;

class Program
{
    //https://github.com/ValwareIRC/valware-unrealircd-mods/blob/main/auto-away/auto-away.c
    //https://modern.ircdocs.horse/#privmsg-message

    private static WebApplicationBuilder _hostBuilder;
    private const string _openApiPath = "/openapi/v1/openapi.json";
    private static DirectoriesConfig _directoriesConfig;

    private static AbyssIrcConfig _config;

    private static WebApplication _app;


    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AbyssIrcOptions))]
    static async Task Main(string[] args)
    {
        AbyssIrcOptions options = null;

        CheckIfRestarted();

        options = ParseOptions(args);


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


        _hostBuilder = WebApplication.CreateBuilder(args);


        if (string.IsNullOrWhiteSpace(options?.RootDirectory))
        {
            options.RootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "abyss");
        }


        SetupRootDirectory(options.RootDirectory);


        _hostBuilder.Services.AddSingleton(
            new AbyssIrcSignalConfig()
            {
                DispatchTasks = Environment.ProcessorCount / 2,
            }
        );
        _config = await LoadConfigAsync(_directoriesConfig.Root, options.ConfigFile);


        Environment.SetEnvironmentVariable("ABYSS_WEB_PORT", _config.WebServer.Port.ToString());


        SetupOpenApi();

        SetupJsonForApi();

        _hostBuilder.Services.AddSingleton(_config.ToServerData());

        if (!string.IsNullOrWhiteSpace(options.HostName))
        {
            Log.Logger.Information("Override hostname to :{HostName}", options.HostName);

            _config.Network.Host = options.HostName;
        }


        _hostBuilder.Services.AddSingleton(_config);


        ConfigureLogging(options);

        // Load plugins

        var pluginManagerService = new PluginManagerService(_directoriesConfig, _config, _hostBuilder);


        pluginManagerService.LoadPlugin(new AbyssServerCorePlugin());
        pluginManagerService.LoadPlugins();


        _hostBuilder.Services.AddHostedService<AbyssIrcHostService>();


        _app = _hostBuilder.Build();


        SetupOpenApi(pluginManagerService);


        await _app.RunAsync();
    }

    private static void SetupRootDirectory(string rootDirectory)
    {
        _directoriesConfig = new DirectoriesConfig(rootDirectory);

        _hostBuilder.Services.AddSingleton(_directoriesConfig);
    }

    private static void CheckIfRestarted()
    {
        var restartFlag = Environment.GetEnvironmentVariable("ABYSS_RESTART");
        var restartReason = Environment.GetEnvironmentVariable("ABYSS_RESTARTREASON");

        if (!string.IsNullOrEmpty(restartFlag) && restartFlag.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Server restarted. Reason: {restartReason ?? "Unspecified"}");

            Environment.SetEnvironmentVariable("ABYSS_RESTART", null);
            Environment.SetEnvironmentVariable("ABYSS_RESTARTREASON", null);
        }
    }

    private static void SetupOpenApi()
    {
        if (_config.WebServer.IsOpenApiEnabled)
        {
            _hostBuilder.Services.AddOpenApi(
                options =>
                {
                    options.AddDocumentTransformer(
                        (document, context, _) =>
                        {
                            document.Info = new()
                            {
                                Title = "AbyssIRC server",
                                Version = "v1",
                                Description = """
                                              AbyssIRC server is a powerful and flexible IRC server implementation.
                                              """,
                                Contact = new()
                                {
                                    Name = "AbyssIRC TEAM",
                                    Url = new Uri("https://github.com/tgiachi/abyssirc-server")
                                }
                            };
                            return Task.CompletedTask;
                        }
                    );
                }
            );

            _hostBuilder.Services.AddEndpointsApiExplorer();
        }

        _hostBuilder.WebHost.UseKestrel(
            s =>
            {
                s.AddServerHeader = false;
                s.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
                s.Listen(
                    new IPEndPoint(_config.WebServer.Host.ToIpAddress(), _config.WebServer.Port),
                    o => { o.Protocols = HttpProtocols.Http1; }
                );
            }
        );
    }

    private static AbyssIrcOptions ParseOptions(string[] args)
    {
        var options = new AbyssIrcOptions();

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

        return options;
    }

    private static void SetupOpenApi(IPluginManagerService pluginManagerService)
    {
        if (_config.WebServer.IsOpenApiEnabled)
        {
            _app.MapOpenApi(_openApiPath).CacheOutput();
            _app.MapScalarApiReference(
                options =>
                {
                    options.OpenApiRoutePattern = _openApiPath;
                    options.Theme = ScalarTheme.BluePlanet;
                }
            );

            Log.Logger.Information(
                "!!! OpenAPI is enabled. You can access the documentation at http://localhost:{Port}/scalar",
                _config.WebServer.Port
            );
        }

        var apiGroup = _app.MapGroup("/api/v1").WithTags("API");


        pluginManagerService.InitializeRoutes(apiGroup);

        MapApiRoutes(apiGroup);


        _app.UseRestAudit();
    }

    private static void SetupJsonForApi()
    {
        _hostBuilder.Services.ConfigureHttpJsonOptions(
            options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonUtils.GetDefaultJsonSettings().PropertyNamingPolicy;
                options.SerializerOptions.WriteIndented = JsonUtils.GetDefaultJsonSettings().WriteIndented;
                options.SerializerOptions.DefaultIgnoreCondition =
                    JsonUtils.GetDefaultJsonSettings().DefaultIgnoreCondition;
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }
        );
    }

    private static void MapApiRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapStatusRoute();
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

    private static void ConfigureLogging(AbyssIrcOptions options)
    {
        var loggingConfig = new LoggerConfiguration()
            .WriteTo.Async(
                s => s.File(
                    formatter: new CompactJsonFormatter(),
                    Path.Combine(_directoriesConfig[DirectoryType.Logs], "abyss_server_.log"),
                    rollingInterval: RollingInterval.Day
                )
            )
            .WriteTo.Async(s => s.Console(theme: AnsiConsoleTheme.Literate));


        //Js log in other file:

        loggingConfig.WriteTo.Logger(
            lc => lc
                .MinimumLevel.Debug()
                .WriteTo.Async(
                    s => s.File(
                        formatter: new CompactJsonFormatter(),
                        path: Path.Combine(_directoriesConfig[DirectoryType.Logs], "js_engine_.log"),
                        rollingInterval: RollingInterval.Day
                    )
                )
                // Filter to include only logs from specific namespaces or with specific properties
                .Filter.ByIncludingOnly(
                    e =>
                        e.Properties.ContainsKey("SourceContext") &&
                        e.Properties["SourceContext"].ToString().Contains("Js") ||
                        e.Properties.ContainsKey("SourceContext").ToString().Contains("JsLogger")
                )
        );

        if (options.EnableDebug)
        {
            loggingConfig.MinimumLevel.Debug();

            // Additional log file for specific logs (e.g., TCP/Network)
            loggingConfig.WriteTo.Logger(
                lc => lc
                    .MinimumLevel.Debug()
                    .WriteTo.Async(
                        s => s.File(
                            formatter: new CompactJsonFormatter(),
                            path: Path.Combine(_directoriesConfig[DirectoryType.Logs], "network_debug_.log"),
                            rollingInterval: RollingInterval.Day
                        )
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

        loggingConfig.MinimumLevel.Override(
            "Microsoft.AspNetCore.Routing.EndpointMiddleware",
            Serilog.Events.LogEventLevel.Warning
        );


        Log.Logger = loggingConfig.CreateLogger();

        _hostBuilder.Logging.ClearProviders().AddSerilog();
    }

    private static void ShowHeader()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "AbyssIrc.Server.Assets.header.txt";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var version = assembly.GetName().Version;

        var customAttribute = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "Codename");


        Console.WriteLine(reader.ReadToEnd());
        Console.WriteLine($"  >> Codename: {customAttribute?.Value ?? "Unknown"}");
        Console.WriteLine($"  >> Version: {version}");
    }
}
