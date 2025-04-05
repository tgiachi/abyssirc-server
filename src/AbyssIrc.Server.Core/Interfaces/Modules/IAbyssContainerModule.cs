using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Core.Interfaces.Modules;

public interface IAbyssContainerModule
{
    IServiceCollection InitializeModule(IServiceCollection services);
}
