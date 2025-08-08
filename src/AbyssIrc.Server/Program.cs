using System.Text;
using AbyssIrc.Core.Json;
using AbyssIrc.Core.Resources;
using AbyssIrc.Server.Core.Bootstrap;
using AbyssIrc.Server.Core.Data.Options;
using AbyssIrc.Server.Core.Json;
using AbyssIrc.Server.Core.Json.Converters;
using ConsoleAppFramework;
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
        string? id = null
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
        };

        var boostrap = new AbyssIrcBoostrap(options, cancellationToken);

        Console.WriteLine("Hello from AbyssIrc Server");

        await boostrap.StartAsync();

        await boostrap.StopAsync();
    }
);
