namespace AbyssIrc.Server.Core.Data.Metrics.ProcessQueue;

public record ProcessQueueMetric(
    string Context,
    int QueuedItems,
    int ProcessedItems,
    int FailedItems,
    TimeSpan AverageProcessingTime
);
