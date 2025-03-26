using AbyssIrc.Core.Attributes.Scripts;
using AbyssIrc.Server.Interfaces.Services.System;
using HamLink.Core.Attributes.Scripts;

namespace AbyssIrc.Server.Modules.Scripts;

[ScriptModule("events")]
public class EventsModule
{
    private readonly IScriptEngineService _scriptEngineService;

    public EventsModule(IScriptEngineService scriptEngineService)
    {
        _scriptEngineService = scriptEngineService;
    }

    [ScriptFunction("Register a callback to be called when the script hamlink is started")]
    public void OnStarted(Action action)
    {
        _scriptEngineService.AddCallback("onStarted", _ => action());
    }
}
