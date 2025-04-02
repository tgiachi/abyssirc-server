using AbyssIrc.Server.Core.Attributes.Scripts;
using AbyssIrc.Server.Core.Interfaces.Services.System;

namespace AbyssIrc.Server.Modules.Scripts;

[ScriptModule("events")]
public class EventsModule
{
    private readonly IScriptEngineService _scriptEngineService;

    private readonly IEventDispatcherService _eventDispatcherService;

    public EventsModule(IScriptEngineService scriptEngineService, IEventDispatcherService eventDispatcherService)
    {
        _scriptEngineService = scriptEngineService;
        _eventDispatcherService = eventDispatcherService;
    }

    [ScriptFunction("Register a callback to be called when the script abyssirc is started")]
    public void OnStarted(Action action)
    {
        _scriptEngineService.AddCallback("onStarted", _ => action());
    }

    [ScriptFunction("Hook into an event")]
    public void HookEvent(string eventName, Action<object?> eventHandler)
    {
        _eventDispatcherService.SubscribeToEvent(eventName, eventHandler.Invoke);
    }
}
