using AbyssIrc.Core.Attributes.Scripts;
using AbyssIrc.Server.Interfaces.Services.System;
using HamLink.Core.Attributes.Scripts;

namespace AbyssIrc.Server.Modules.Scripts;

[ScriptModule("scheduler")]
public class SchedulerModule
{
    private readonly ISchedulerSystemService _schedulerSystemService;

    public SchedulerModule(ISchedulerSystemService schedulerSystemService)
    {
        _schedulerSystemService = schedulerSystemService;
    }

    [ScriptFunction("Schedule a task to be run every x seconds")]
    public void ScheduleTask(string name, int seconds, Action callback)
    {
        _schedulerSystemService.RegisterJob(
            name,
            () =>
            {
                callback();
                return Task.CompletedTask;
            },
            TimeSpan.FromSeconds(seconds)
        );
    }
}
