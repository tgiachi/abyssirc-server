namespace AbyssIrc.Server.Interfaces.Services;

public interface ISchedulerSystemService : IDisposable
{
    Task RegisterJob(string name, Func<Task> task, TimeSpan interval);
    Task UnregisterJob(string name);
    Task<bool> IsJobRegistered(string name);
    Task PauseJob(string name);
    Task ResumeJob(string name);
}
