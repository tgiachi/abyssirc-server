using AbyssIrc.Server.Core.Data.Plugins;
using Microsoft.AspNetCore.Routing;

namespace AbyssIrc.Server.Core.Interfaces.Services.System;

public interface IPluginManagerService
{
    /// <summary>
    ///     Loads all plugins from the specified directory.
    /// </summary>
    void LoadPlugins();

    /// <summary>
    ///     Gets the loaded plugins.
    /// </summary>
    IEnumerable<AbyssPluginInfo> LoadedPlugins { get; }

    /// <summary>
    ///     Gets the plugin info for a specific plugin.
    /// </summary>
    AbyssPluginInfo GetPluginInfo(string pluginName);


    /// <summary>
    ///   Initializes the plugin with the api routes
    /// </summary>
    /// <param name="routeGroupBuilder"></param>
    void InitializeRoutes(RouteGroupBuilder routeGroupBuilder);
}
