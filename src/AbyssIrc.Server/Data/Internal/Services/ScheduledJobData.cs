namespace AbyssIrc.Server.Data.Internal.Services;

public class ScheduledJobData
{
    public string Name { get; set; }
    public TimeSpan Interval { get; set; }
    public Func<Task> Task { get; set; }
    public IDisposable Subscription { get; set; }
}
