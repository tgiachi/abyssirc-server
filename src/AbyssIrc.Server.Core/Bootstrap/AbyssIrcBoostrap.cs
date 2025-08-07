using AbyssIrc.Server.Core.Data.Options;
using DryIoc;

namespace AbyssIrc.Server.Core.Bootstrap;

public class AbyssIrcBoostrap
{
    private readonly IContainer _container;


    public AbyssIrcBoostrap(AbyssIrcOptions options)
    {
        _container = new Container(rules => rules
            // Use interpreted mode instead of expression compilation
            .WithoutInterpretationForTheFirstResolution()
            .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
            .WithoutEagerCachingSingletonForFasterAccess()
            // Use factory delegates instead of expression trees
            .WithFactorySelector(Rules.SelectLastRegisteredFactory())
            // Optimize for AOT
            .WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient)
            .WithAutoConcreteTypeResolution()
        );
    }

    private void Init()
    {

    }


}
