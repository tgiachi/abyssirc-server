using AbyssIrc.Server.Core.Attributes.Scripts;
using AbyssIrc.Server.Core.Interfaces.Services.System;

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
