using System.Collections.Concurrent;

namespace AbyssIrc.Server.Data.Metrics;

public class ProcessStats
{
    private long _queuedItems;
    private long _processedItems;
    private long _failedItems;
    private readonly ConcurrentQueue<TimeSpan> _processingTimes = new();
    private const int MaxProcessingTimesSamples = 100;

    public int QueuedItems => (int)Interlocked.Read(ref _queuedItems);
    public int ProcessedItems => (int)Interlocked.Read(ref _processedItems);
    public int FailedItems => (int)Interlocked.Read(ref _failedItems);

    public TimeSpan AverageProcessingTime
    {
        get
        {
            if (_processingTimes.IsEmpty)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromTicks((long)_processingTimes.Average(t => t.Ticks));
        }
    }

    public void IncrementQueued() => Interlocked.Increment(ref _queuedItems);

    public void IncrementProcessed(TimeSpan processingTime)
    {
        Interlocked.Increment(ref _processedItems);
        _processingTimes.Enqueue(processingTime);
        while (_processingTimes.Count > MaxProcessingTimesSamples)
        {
            _processingTimes.TryDequeue(out _);
        }
    }

    public void IncrementFailed() => Interlocked.Increment(ref _failedItems);
}
