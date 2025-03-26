using AbyssIrc.Core.Extensions;
using AbyssIrc.Network.Data.Internal;
using AbyssIrc.Network.Interfaces.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Extensions;

public static class RegisterIrcCommandExtension
{
    public static IServiceCollection RegisterIrcCommand(this IServiceCollection services, Type commandType)
    {
        var command = Activator.CreateInstance(commandType) as IIrcCommand;

        if (command == null)
        {
            throw new InvalidOperationException($"Type {commandType} does not implement {nameof(IIrcCommand)}");
        }

        services.AddToRegisterTypedList(new IrcCommandDefinitionData(command));

        return services;
    }
}
