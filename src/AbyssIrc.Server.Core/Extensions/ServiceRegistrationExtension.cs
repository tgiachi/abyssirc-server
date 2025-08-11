using AbyssIrc.Core.Extensions;
using AbyssIrc.Server.Core.Data.Internal.Services;
using DryIoc;

namespace AbyssIrc.Server.Core.Extensions;

public static class ServiceRegistrationExtension
{
    public static IContainer RegisterService(
        this IContainer container, Type serviceType, Type implementationType, int priority = 0
    )
    {
        ArgumentNullException.ThrowIfNull(container);

        ArgumentNullException.ThrowIfNull(serviceType);

        ArgumentNullException.ThrowIfNull(implementationType);

        container.Register(serviceType, implementationType, Reuse.Singleton);

        container.AddToRegisterTypedList(new ServiceDefinitionObject(serviceType, implementationType, priority));

        return container;
    }

    public static IContainer RegisterService(this IContainer container, Type serviceType, int priority = 0)
    {
        return RegisterService(container, serviceType, serviceType, priority);
    }
}
