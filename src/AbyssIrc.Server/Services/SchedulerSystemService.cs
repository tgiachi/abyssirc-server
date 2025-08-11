using System.Collections.Concurrent;
using System.Diagnostics;
using AbyssIrc.Server.Core.Data.Scheduler;
using AbyssIrc.Server.Core.Interfaces.EventBus;
using AbyssIrc.Server.Core.Interfaces.Services;
using AbyssIrc.Server.Events.Scheduler;
using Serilog;

namespace AbyssIrc.Server.Services;

public class SchedulerSystemService : ISchedulerSystemService, IEventBusListener<AddSchedulerJobEvent>
{
    private readonly ILogger _logger = Log.ForContext<SchedulerSystemService>();
    private readonly ConcurrentDictionary<string, ScheduledJobData> _jobs;
    private readonly ConcurrentDictionary<string, IDisposable> _pausedJobs;

    public SchedulerSystemService(IEventBusService eventBusService)
    {
        _jobs = new ConcurrentDictionary<string, ScheduledJobData>();
        _pausedJobs = new ConcurrentDictionary<string, IDisposable>();
        eventBusService.Subscribe(this);
    }


    public async Task RegisterJob(string name, Func<Task> task, TimeSpan interval)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Job name cannot be empty", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(task);

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentException("Interval must be positive", nameof(interval));
        }

        if (await IsJobRegistered(name))
        {
            throw new InvalidOperationException($"Job '{name}' is already registered");
        }

        var subscription = System.Reactive.Linq.Observable
            .Interval(interval)
            .Subscribe(async _ =>
                {
                    try
                    {
                        await ExecuteJob(_jobs[name]);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it according to your needs
                        _logger.Error(ex, "Error occurred while executing job '{JobName}'", name);
                    }
                }
            );

        var job = new ScheduledJobData
        {
            Name = name,
            Interval = interval,
            Task = task,
            Subscription = subscription
        };

        _jobs.TryAdd(name, job);
    }

    public async Task UnregisterJob(string name)
    {
        if (await IsJobRegistered(name))
        {
            if (_jobs.TryRemove(name, out var job))
            {
                job.Subscription?.Dispose();
            }
        }
    }

    public Task<bool> IsJobRegistered(string name)
    {
        return Task.FromResult(_jobs.ContainsKey(name));
    }

    public async Task PauseJob(string name)
    {
        if (!await IsJobRegistered(name))
        {
            throw new InvalidOperationException($"Job '{name}' is not registered");
        }

        if (_jobs.TryGetValue(name, out var job))
        {
            job.Subscription?.Dispose();
            _pausedJobs.TryAdd(name, job.Subscription);
        }
    }

    private async Task ExecuteJob(ScheduledJobData jobData)
    {
        var startTime = Stopwatch.GetTimestamp();
        _logger.Verbose("Executing job '{JobName}'", jobData.Name);
        await jobData.Task();
        var elapsed = Stopwatch.GetElapsedTime(startTime);

        _logger.Verbose("Job '{JobName}' executed in {Elapsed} ms", jobData.Name, elapsed);
    }

    public async Task ResumeJob(string name)
    {
        if (!await IsJobRegistered(name))
        {
            throw new InvalidOperationException($"Job '{name}' is not registered");
        }

        if (_jobs.TryGetValue(name, out var job))
        {
            var subscription = System.Reactive.Linq.Observable
                .Interval(job.Interval)
                .Subscribe(async _ => await ExecuteJob(_jobs[name]));

            job.Subscription = subscription;
            _pausedJobs.TryRemove(name, out _);
        }
    }

    public void Dispose()
    {
        foreach (var job in _jobs.Values)
        {
            job.Subscription?.Dispose();
        }

        _jobs.Clear();
        _pausedJobs.Clear();
        GC.SuppressFinalize(this);
    }


    public async Task HandleAsync(AddSchedulerJobEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.Information("Registering job '{JobName}'", @event.Name);
        _ = RegisterJob(@event.Name, @event.Action, @event.TotalSpan);
    }
}
