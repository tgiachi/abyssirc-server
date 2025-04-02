namespace AbyssIrc.Server.Data.Metrics;

public record ProcessQueueMetric(
    string Context,
    int QueuedItems,
    int ProcessedItems,
    int FailedItems,
    TimeSpan AverageProcessingTime
);
