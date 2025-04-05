using AbyssIrc.Server.Core.Data.Plugins;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Core.Interfaces.Plugins;

public interface IAbyssIrcPlugin
{
    AbyssPluginInfo PluginInfo { get; }
    void Initialize(IServiceCollection services);
    void InitializeRoutes(RouteGroupBuilder routeGroupBuilder);
}
