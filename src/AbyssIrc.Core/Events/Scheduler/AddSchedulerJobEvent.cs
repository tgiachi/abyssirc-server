namespace AbyssIrc.Core.Events.Scheduler;

public record AddSchedulerJobEvent(string Name, TimeSpan TotalSpan, Func<Task> Action);
