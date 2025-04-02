namespace AbyssIrc.Server.Core.Events.Scheduler;

public record AddSchedulerJobEvent(string Name, TimeSpan TotalSpan, Func<Task> Action);
