using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Core.Utils;
using AbyssIrc.Server.Core.Data.Configs;
using AbyssIrc.Server.Core.Data.Configs.Sections.Oper;
using AbyssIrc.Server.Core.Data.Directories;
using AbyssIrc.Server.Core.Extensions;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Core.Types;
using AbyssIrc.Server.Data.Options;
using AbyssIrc.Server.Extensions;
using AbyssIrc.Server.Middleware;
using AbyssIrc.Server.Plugins.Core;
using AbyssIrc.Server.Routes;
using AbyssIrc.Server.Services;
using AbyssIrc.Server.Services.Hosting;
using AbyssIrc.Signals.Data.Configs;
using CommandLine;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace AbyssIrc.Server;

public class Program
{

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AbyssIrcOptions))]
    static async Task Main(string[] args)
    {
        var startup = new Startup();
        await startup.RunAsync(args);
    }
}

public class Startup
{
    private const string OpenApiPath = "/openapi/v1/openapi.json";

    private WebApplicationBuilder _hostBuilder;
    private DirectoriesConfig _directoriesConfig;
    private AbyssIrcConfig _config;
    private WebApplication _app;
    private IPluginManagerService _pluginManagerService;

    public async Task RunAsync(string[] args)
    {
        try
        {
            var options = ParseCommandLineOptions(args);
            CheckIfRestarted();

            if (options.ShowHeader)
            {
                ShowHeader();
            }

            ApplyEnvironmentVariables(options);

            _hostBuilder = WebApplication.CreateBuilder(args);

            InitializeRootDirectory(options.RootDirectory);

            _config = await LoadAndInitializeConfigAsync(options);

            ConfigureServices(options);

            _app = _hostBuilder.Build();

            ConfigureMiddleware();

            await _app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            throw;
        }
    }

    private static AbyssIrcOptions ParseCommandLineOptions(string[] args)
    {
        var options = new AbyssIrcOptions();

        Parser.Default.ParseArguments<AbyssIrcOptions>(args)
            .WithParsed(ircOptions => { options = ircOptions; })
            .WithNotParsed(errors =>
            {
                Environment.Exit(1);
                return;
            });

        return options;
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

    private static void ApplyEnvironmentVariables(AbyssIrcOptions options)
    {
        if (Environment.GetEnvironmentVariable("ABYSS_ROOT_DIRECTORY") != null)
        {
            options.RootDirectory = Environment.GetEnvironmentVariable("ABYSS_ROOT_DIRECTORY");
        }

        if (Environment.GetEnvironmentVariable("ABYSS_ENABLE_DEBUG") != null)
        {
            options.EnableDebug = true;
        }

        if (string.IsNullOrWhiteSpace(options?.RootDirectory))
        {
            options.RootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "abyss");
        }
    }

    private void InitializeRootDirectory(string rootDirectory)
    {
        _directoriesConfig = new DirectoriesConfig(rootDirectory);
        _hostBuilder.Services.AddSingleton(_directoriesConfig);
    }

    private async Task<AbyssIrcConfig> LoadAndInitializeConfigAsync(AbyssIrcOptions options)
    {
        var config = await LoadConfigAsync(_directoriesConfig.Root, options.ConfigFile);

        EnsureDefaultOperExists(config);

        await VerifyNonCryptedPasswordOpers(_directoriesConfig.Root, options.ConfigFile, config);

        Environment.SetEnvironmentVariable("ABYSS_WEB_PORT", config.WebServer.Port.ToString());

        if (!string.IsNullOrWhiteSpace(options.HostName))
        {
            Log.Logger.Information("Override hostname to :{HostName}", options.HostName);
            config.Network.Host = options.HostName;
        }

        return config;
    }

    private static void EnsureDefaultOperExists(AbyssIrcConfig config)
    {
        if (config.Opers.Users.Count == 0)
        {
            Console.WriteLine("No users have been configured.");
            Console.WriteLine("!!! Adding default user: admin/admin and can use webServer");

            config.Opers.Users.Add(
                new OperEntry()
                {
                    Host = "*",
                    Username = "admin",
                    Password = "admin",
                    CanUseWebServer = true
                }
            );
        }
    }

    private void ConfigureServices(AbyssIrcOptions options)
    {
        _hostBuilder.Services.AddSingleton(
            new AbyssIrcSignalConfig()
            {
                DispatchTasks = Environment.ProcessorCount / 2,
            }
        );

        _hostBuilder.Services.AddSingleton(_config.ToServerData());
        _hostBuilder.Services.AddSingleton(_config);

        ConfigureAuthentication();
        ConfigureOpenApi();
        ConfigureJsonForApi();
        ConfigureLogging(options);

        LoadPlugins();

        _hostBuilder.Services.AddHostedService<AbyssIrcHostService>();
    }

    private void ConfigureAuthentication()
    {
        _hostBuilder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _config.WebServer.JwtAuthConfig.Issuer,
                        ValidAudience = _config.WebServer.JwtAuthConfig.Audience,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(_config.WebServer.JwtAuthConfig.Secret.FromBase64ToByteArray())
                    };
                }
            );

        _hostBuilder.Services.AddAuthorization();
    }

    private void ConfigureOpenApi()
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

    private void ConfigureJsonForApi()
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

    private void LoadPlugins()
    {
        _pluginManagerService = new PluginManagerService(_directoriesConfig, _config, _hostBuilder);

        _pluginManagerService.LoadPlugin(new AbyssServerCorePlugin());
        _pluginManagerService.LoadPlugins();
    }

    private void ConfigureMiddleware()
    {
        _app.UseAuthorization();

        ConfigureOpenApiMiddleware();

        var apiGroup = _app.MapGroup("/api/v1").WithTags("API");

        _pluginManagerService.InitializeRoutes(apiGroup);

        MapApiRoutes(apiGroup);

        _app.UseRestAudit();
    }

    private void ConfigureOpenApiMiddleware()
    {
        if (_config.WebServer.IsOpenApiEnabled)
        {
            _app.MapOpenApi(OpenApiPath).CacheOutput();
            _app.MapScalarApiReference(
                options =>
                {
                    options.OpenApiRoutePattern = OpenApiPath;
                    options.Theme = ScalarTheme.BluePlanet;
                }
            );

            Log.Logger.Information(
                "!!! OpenAPI is enabled. You can access the documentation at http://localhost:{Port}/scalar",
                _config.WebServer.Port
            );
        }
    }

    private void MapApiRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup
            .MapStatusRoute()
            .MapAuthRoute();
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

    private static Task SaveConfigAsync(string rootDirectory, string configFileName, AbyssIrcConfig config)
    {
        var configFile = Path.Combine(rootDirectory, configFileName);

        return File.WriteAllTextAsync(configFile, config.ToYaml());
    }

    private async Task VerifyNonCryptedPasswordOpers(
        string rootDirectory, string configFileName, AbyssIrcConfig config
    )
    {
        var needSave = false;
        foreach (var oper in config.Opers.Users)
        {
            if (!oper.Password.StartsWith("hash:"))
            {
                var (hash, salt) = HashUtils.HashPassword(oper.Password);
                oper.Password = "hash:" + hash + ":" + salt;
                needSave = true;
            }
        }

        if (needSave)
        {
            await SaveConfigAsync(rootDirectory, configFileName, config);
        }
    }

    private void ConfigureLogging(AbyssIrcOptions options)
    {
        var loggingConfig = CreateBaseLoggingConfiguration();

        ConfigureJsLogging(loggingConfig);

        if (options.EnableDebug)
        {
            ConfigureDebugLogging(loggingConfig);
        }
        else
        {
            loggingConfig.MinimumLevel.Information();
        }

        ConfigureLogLevelOverrides(loggingConfig);

        Log.Logger = loggingConfig.CreateLogger();

        _hostBuilder.Logging.ClearProviders().AddSerilog();
    }

    private LoggerConfiguration CreateBaseLoggingConfiguration()
    {
        return new LoggerConfiguration()
            .WriteTo.Async(
                s => s.File(
                    formatter: new CompactJsonFormatter(),
                    Path.Combine(_directoriesConfig[DirectoryType.Logs], "abyss_server_.log"),
                    rollingInterval: RollingInterval.Day
                )
            )
            .WriteTo.Async(s => s.Console(theme: AnsiConsoleTheme.Literate));
    }

    private void ConfigureJsLogging(LoggerConfiguration loggingConfig)
    {
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
                .Filter.ByIncludingOnly(
                    e =>
                        e.Properties.ContainsKey("SourceContext") &&
                        e.Properties["SourceContext"].ToString().Contains("Js") ||
                        e.Properties.ContainsKey("SourceContext").ToString().Contains("JsLogger")
                )
        );
    }

    private void ConfigureDebugLogging(LoggerConfiguration loggingConfig)
    {
        loggingConfig.MinimumLevel.Debug();

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
                .Filter.ByIncludingOnly(
                    e =>
                        e.Properties.ContainsKey("SourceContext") &&
                        e.Properties["SourceContext"].ToString().Contains("Tcp") ||
                        e.Properties.ContainsKey("SourceContext").ToString().Contains("TcpService")
                )
        );
    }

    private static void ConfigureLogLevelOverrides(LoggerConfiguration loggingConfig)
    {
        loggingConfig.MinimumLevel.Override(
            "Microsoft.AspNetCore.Routing.EndpointMiddleware",
            Serilog.Events.LogEventLevel.Warning
        );
    }

    private void ShowHeader()
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
