using AbyssIrc.Server.Core.Data.Options;
using DryIoc;

namespace AbyssIrc.Server.Core.Bootstrap;

public class AbyssIrcBoostrap
{
    private readonly IContainer _container;

    private readonly AbyssIrcOptions _options;

    private readonly CancellationTokenSource _cancellationTokenSource;

    public delegate IContainer RegisterHandler(IContainer container);

    public event RegisterHandler OnRegister;


    public AbyssIrcBoostrap(AbyssIrcOptions options, CancellationToken cancellationToken)
    {
        _options = options;
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

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

    public async Task StartAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            await Task.Delay(1000, _cancellationTokenSource.Token);
        }

    }

    public async Task StopAsync()
    {
        Console.WriteLine("Stopping AbyssIrc Server");
    }

    private void Init()
    {
    }
}
