using AbyssIrc.Core.Extensions;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Internal;
using AbyssIrc.Server.Data.Internal.ServiceCollection;
using AbyssIrc.Server.Interfaces.Listener;
using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Extensions;

public static class RegisterIrcCommandListenerExtension
{
    public static IServiceCollection RegisterIrcCommandListener(
        this IServiceCollection services, Type handlerType, IIrcCommand ircCommand
    )
    {
        services.AddSingleton(handlerType);

        services.AddToRegisterTypedList(new IrcCommandListenerDefinitionData(handlerType, ircCommand));

        return services;
    }

    public static IServiceCollection RegisterIrcCommandListener<THandler>(
        this IServiceCollection services, IIrcCommand ircCommand
    ) where THandler : class, IIrcMessageListener
    {
        services.AddSingleton<THandler>();

        services.AddToRegisterTypedList(new IrcCommandListenerDefinitionData(typeof(THandler), ircCommand));

        return services;
    }
}
