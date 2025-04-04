using System.Reflection;
using AbyssIrc.Server.Core.Data.Configs;
using AbyssIrc.Server.Core.Data.Directories;
using AbyssIrc.Server.Core.Data.Plugins;
using AbyssIrc.Server.Core.Interfaces.Plugins;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Core.Types;

namespace AbyssIrc.Server.Services;

public class PluginManagerService : IPluginManagerService
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<PluginManagerService>();

    private readonly DirectoriesConfig _directoriesConfig;

    private readonly AbyssIrcConfig _abyssIrcConfig;

    private readonly WebApplicationBuilder _webApplicationBuilder;

    private readonly List<AbyssPluginInfo> _availablePlugins = new();

    private readonly List<IAbyssIrcPlugin> _plugins = new();

    public PluginManagerService(
        DirectoriesConfig directoriesConfig, AbyssIrcConfig abyssIrcConfig, WebApplicationBuilder webApplicationBuilder
    )
    {
        _directoriesConfig = directoriesConfig;
        _abyssIrcConfig = abyssIrcConfig;
        _webApplicationBuilder = webApplicationBuilder;

        _webApplicationBuilder.Services.AddSingleton<IPluginManagerService>(this);
    }

    public IEnumerable<AbyssPluginInfo> LoadedPlugins { get; set; } = new List<AbyssPluginInfo>();

    public void LoadPlugins()
    {
        var directory = _directoriesConfig[DirectoryType.Plugins];

        _logger.Information("Loading plugins from {directory}", directory);

        var pluginFiles = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories);
        var tempPluginInfos = new Dictionary<string, (AbyssPluginInfo Info, IAbyssIrcPlugin Plugin, string Path)>();

        _logger.Information("Found {Count} plugins", pluginFiles.Length);

        foreach (var pluginFile in pluginFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(pluginFile);

                var pluginInfoType = assembly.GetTypes()
                    .FirstOrDefault(t => t.IsClass && !t.IsAbstract && typeof(IAbyssIrcPlugin).IsAssignableFrom(t));

                if (pluginInfoType != null)
                {
                    var pluginInstance = (IAbyssIrcPlugin)Activator.CreateInstance(pluginInfoType)!;
                    var pluginInfo = pluginInstance.PluginInfo;
                    tempPluginInfos[pluginInfo.Id] = (pluginInfo, pluginInstance, pluginFile);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to read plugin info from {pluginFile}", pluginFile);
            }
        }

        var sortedPluginInfos = TopologicalSort(tempPluginInfos.Values.Select(p => p.Info).ToList());


        foreach (var pluginToLoad in _abyssIrcConfig.Plugins.Entries.Where(s => s.IsEnabled))
        {
            var pluginPath = sortedPluginInfos.FirstOrDefault(p => p.Id == pluginToLoad.PluginId);

            if (!tempPluginInfos.TryGetValue(pluginToLoad.PluginId, out var _))
            {
                throw new Exception($"Plugin {pluginToLoad.PluginId} not found");
            }

            var pluginInfo = pluginPath;

            var pluginIns = tempPluginInfos[pluginToLoad.PluginId].Plugin;

            try
            {
                _availablePlugins.Add(pluginInfo);
                _logger.Information("Loaded plugin {PluginId}", pluginInfo.Id);

                try
                {
                    var pluginInstance = pluginIns;
                    pluginInstance.Initialize(_webApplicationBuilder.Services);

                    _logger.Information("Initialized plugin {PluginId}", pluginInfo.Id);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to initialize plugin {PluginId}", pluginInfo.Id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to activate plugin {PluginId}", pluginInfo.Id);
            }
        }

        LoadedPlugins = _availablePlugins;
    }


    public AbyssPluginInfo? GetPluginInfo(string pluginId)
    {
        var pluginInfo = LoadedPlugins.FirstOrDefault(p => p.Id == pluginId);

        if (pluginInfo == null)
        {
            _logger.Warning("Plugin with id {pluginId} not found", pluginId);
            return null;
        }

        return pluginInfo;
    }

    public void InitializeRoutes(RouteGroupBuilder routeGroupBuilder)
    {
        foreach (var plugin in _plugins)
        {
            try
            {
                plugin.InitializeRoutes(routeGroupBuilder);
                _logger.Debug("Initialized routes for plugin {PluginId}", plugin.PluginInfo.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize routes for plugin {PluginId}", plugin.PluginInfo.Id);
            }
        }
    }

    private static List<AbyssPluginInfo> TopologicalSort(List<AbyssPluginInfo> plugins)
    {
        var sorted = new List<AbyssPluginInfo>();
        var visited = new Dictionary<string, bool>();

        foreach (var plugin in plugins)
        {
            Visit(plugin);
        }

        return sorted;

        void Visit(AbyssPluginInfo plugin)
        {
            if (visited.TryGetValue(plugin.Id, out var inProcess))
            {
                if (inProcess)
                {
                    throw new Exception($"Cyclic dependency detected at plugin {plugin.Id}");
                }

                return; // already visited
            }

            visited[plugin.Id] = true;

            foreach (var depId in plugin.Dependencies ?? [])
            {
                var dep = plugins.FirstOrDefault(p => p.Id == depId);
                if (dep == null)
                {
                    throw new Exception($"Missing dependency {depId} for plugin {plugin.Id}");
                }

                Visit(dep);
            }

            visited[plugin.Id] = false;
            sorted.Add(plugin);
        }
    }
}
