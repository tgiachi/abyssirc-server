using System.Text;
using AbyssIrc.Core.Json;
using AbyssIrc.Core.Resources;
using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Interfaces.Parser;
using AbyssIrc.Protocol.Messages.Services;
using AbyssIrc.Server.Core.Bootstrap;
using AbyssIrc.Server.Core.Data.Internal.Services;
using AbyssIrc.Server.Core.Data.Options;
using AbyssIrc.Server.Core.Data.Services;
using AbyssIrc.Server.Core.Extensions;
using AbyssIrc.Server.Core.Interfaces.Services;
using AbyssIrc.Server.Core.Json;
using AbyssIrc.Server.Core.Json.Converters;
using AbyssIrc.Server.Core.Modules;
using AbyssIrc.Server.Core.Types.Logger;
using AbyssIrc.Server.Services;
using ConsoleAppFramework;
using DryIoc;
using NanoidDotNet;
using AbyssIrcJsonContext = AbyssIrc.Server.Core.Json.Context.AbyssIrcJsonContext;

JsonUtils.RegisterJsonContext(AbyssIrcJsonContext.Default);


await ConsoleApp.RunAsync(
    args,
    async (
        CancellationToken cancellationToken,
        bool showHeader = true,
        string? rootDirectory = null,
        string? securePorts = null,
        string? nonSecurePorts = null,
        string? certificatePath = null,
        string? certificatePassword = null,
        string? id = null,
        string? configFile = null,
        bool logToConsole = true,
        bool logToFile = true,
        LogLevelType logLevelType = LogLevelType.Debug
    ) =>
    {
        if (showHeader)
        {
            var header = ResourceUtils.GetEmbeddedResourceContent("Assets/_header.txt", typeof(Program).Assembly);

            Console.WriteLine(Encoding.UTF8.GetString(header));
            Console.WriteLine();
        }

        var options = new AbyssIrcOptions()
        {
            Id = id ?? Nanoid.Generate(),
            RootDirectory = rootDirectory ?? Environment.GetEnvironmentVariable("ABYSSIRC_ROOTDIRECTORY"),
            CertificatePath = certificatePath ?? Environment.GetEnvironmentVariable("ABYSSIRC_CERTIFICATE"),
            CertificatePassword = certificatePassword ?? Environment.GetEnvironmentVariable("ABYSSIRC_CERTIFICATE_PASSWORD"),
            SecurePorts = NumberRangeConverter.ParseNumberRange(
                securePorts ?? Environment.GetEnvironmentVariable("ABYSSIRC_SECURE_PORTS")
            ),
            NonSecurePorts = NumberRangeConverter.ParseNumberRange(
                nonSecurePorts ?? Environment.GetEnvironmentVariable("ABYSSIRC_NON_SECURE_PORTS")
            ),
            Config = configFile ?? Environment.GetEnvironmentVariable("ABYSSIRC_CONFIG_FILE"),
            LogToConsole = logToConsole,
            LogToFile = logToFile,
            LogLevel = logLevelType
        };

        if (string.IsNullOrEmpty(options.Config))
        {
            options.Config = "config.json";
        }

        if (string.IsNullOrEmpty(options.RootDirectory))
        {
            options.RootDirectory = Path.Combine(Environment.CurrentDirectory, "abyssirc");
        }

        var boostrap = new AbyssIrcBoostrap(options, cancellationToken);

        boostrap.OnRegisterServices += container =>
        {
            container
                .RegisterService(typeof(IDiagnosticService), typeof(DiagnosticService))
                .RegisterService(typeof(ISchedulerSystemService), typeof(SchedulerSystemService))
                .RegisterService(typeof(IEventBusService), typeof(EventBusService))
                .RegisterService(typeof(IEventDispatcherService), typeof(EventDispatcherService))
                .RegisterService(typeof(IVersionService), typeof(VersionService))
                .RegisterService(typeof(IScriptEngineService), typeof(JsScriptEngineService))
                .RegisterService(typeof(IProcessQueueService), typeof(ProcessQueueService))
                .RegisterService(typeof(INetworkService), typeof(NetworkService), 100)
                ;

            container
                .RegisterService(typeof(IIrcCommandParser), typeof(IrcCommandParser));


            // registering config


            container.RegisterInstance(
                new DiagnosticServiceConfig()
                {
                    MetricsIntervalInSeconds = 60,
                    PidFileName = "abyssirc.pid"
                }
            );

            container.RegisterInstance(new ScriptEngineConfig());

            container.RegisterInstance(new ProcessQueueConfig());

            container.AddScriptModule(typeof(LoggerModule));


            return container;
        };

        boostrap.OnNetworkServices += service =>
        {
            service.RegisterCommand<CapCommand>();
            service.RegisterCommand<UserCommand>();
            service.RegisterCommand<NickCommand>();
        };


        boostrap.Init();

        Console.WriteLine("Hello from AbyssIrc Server");

        await boostrap.StartAsync();

        await boostrap.StopAsync();
    }
);
