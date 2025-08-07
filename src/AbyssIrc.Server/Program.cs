using System.Text;
using AbyssIrc.Core.Json;
using AbyssIrc.Core.Resources;
using AbyssIrc.Server.Core.Json;
using ConsoleAppFramework;

JsonUtils.RegisterJsonContext(AbyssIrcJsonContext.Default);


await ConsoleApp.RunAsync(
    args,
    async (string rootDirectory = null, bool showHeader = true, CancellationToken cancellationToken = default) =>
    {
        if (showHeader)
        {
            var header = ResourceUtils.GetEmbeddedResourceContent("Assets/_header.txt", typeof(Program).Assembly);

            Console.WriteLine(Encoding.UTF8.GetString(header));
            Console.WriteLine();
        }

        Console.WriteLine("Hello from AbyssIrc Server");
    }
);
