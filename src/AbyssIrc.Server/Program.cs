using AbyssIrc.Core.Json;
using AbyssIrc.Server.Core.Json;
using ConsoleAppFramework;

JsonUtils.RegisterJsonContext(AbyssIrcJsonContext.Default);


await ConsoleApp.RunAsync(
    args,
    async ( CancellationToken cancellationToken) =>
    {

        Console.WriteLine("Hello from AbyssIrc Server");
    }
);
