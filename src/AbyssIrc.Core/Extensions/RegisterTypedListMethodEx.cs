using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Core.Extensions;

public static class RegisterTypedListMethodEx
{
    /// <summary>
    ///   Add to register typed list
    ///   for example: if you have a list of entities that you want to register in the DI container
    /// </summary>
    /// <param name="services"></param>
    /// <param name="entity"></param>
    /// <typeparam name="TListEntity"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddToRegisterTypedList<TListEntity>(
        this IServiceCollection services, TListEntity entity
    )
    {
        var typedList = new List<TListEntity>();

        if (services.Any(x => x.ServiceType == typeof(List<TListEntity>)))
        {
            // get list of service definitions
            var serviceDefinitions = services.First(x => x.ServiceType == typeof(List<TListEntity>));
            typedList = (List<TListEntity>)serviceDefinitions.ImplementationInstance;
            typedList.Add(entity);
        }
        else
        {
            // add list of service definitions
            typedList.Add(entity);
            services.AddSingleton(typeof(List<TListEntity>), typedList);
            return services;
        }

        return services;
    }
}
