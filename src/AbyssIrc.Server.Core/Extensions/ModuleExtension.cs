using AbyssIrc.Server.Core.Interfaces.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Core.Extensions;

public static class ModuleExtension
{

    public static IServiceCollection AddModule<T>(this IServiceCollection services) where T : class, IAbyssContainerModule
    {
        var module = Activator.CreateInstance<T>();
        return module.InitializeModule(services);
    }

    public static IServiceCollection AddModule(this IServiceCollection services, Type moduleType)
    {
        if (!typeof(IAbyssContainerModule).IsAssignableFrom(moduleType))
        {
            throw new ArgumentException($"Type {moduleType.Name} does not implement {nameof(IAbyssContainerModule)}");
        }

        var module = (IAbyssContainerModule)Activator.CreateInstance(moduleType);
        return module.InitializeModule(services);
    }
}
