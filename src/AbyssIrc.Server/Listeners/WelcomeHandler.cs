using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;

namespace AbyssIrc.Server.Listeners;

public class WelcomeHandler : BaseHandler, IAbyssSignalListener<ClientReadyEvent>
{
    public WelcomeHandler(IAbyssSignalService signalService) : base(signalService)
    {
        signalService.Subscribe(this);
    }

    public async Task OnEventAsync(ClientReadyEvent signalEvent)
    {
    }
}
