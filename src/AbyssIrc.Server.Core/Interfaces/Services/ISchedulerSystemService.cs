using AbyssIrc.Core.Interfaces.Services;

namespace AbyssIrc.Server.Core.Interfaces.Services;

public interface ISchedulerSystemService : IAbyssService
{
    Task RegisterJob(string name, Func<Task> task, TimeSpan interval);
    Task UnregisterJob(string name);
    Task<bool> IsJobRegistered(string name);
    Task PauseJob(string name);
    Task ResumeJob(string name);
}
