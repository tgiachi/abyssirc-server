using System.Reflection;
using AbyssIrc.Server.Core.Data.Plugins;
using AbyssIrc.Server.Core.Extensions;
using AbyssIrc.Server.Core.Interfaces.Plugins;
using AbyssIrc.Server.Plugins.Core.Modules;

namespace AbyssIrc.Server.Plugins.Core;

public class AbyssServerCorePlugin : IAbyssIrcPlugin
{
    public AbyssPluginInfo PluginInfo { get; }

    public AbyssServerCorePlugin()
    {
        // get current version
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        PluginInfo = new AbyssPluginInfo(
            Id: "AbyssServerCore",
            Name: "Abyss Server Core",
            Version: version?.ToString() ?? "0.0.0",
            Description: "Core plugin for Abyss Irc Server.",
            Authors: "Abyss Irc Team",
            Dependencies: []
        );
    }

    public void Initialize(IServiceCollection services)
    {
        services
            .AddModule<CoreServiceContainerModule>()
            .AddModule<MessageAndHandlerContainerModule>()
            .AddModule<ScriptModuleContainer>();
    }

    public void InitializeRoutes(RouteGroupBuilder routeGroupBuilder)
    {
        throw new NotImplementedException();
    }
}
