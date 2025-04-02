using System.Diagnostics;
using Humanizer;

namespace AbyssIrc.Server.Core.Data.Metrics;

public record DiagnosticMetrics
{
    // Memory metrics
    public long PrivateMemoryBytes { get; init; }
    public long PagedMemoryBytes { get; init; }
    public long ManagedMemoryBytes { get; init; }

    // Process metrics
    public int ThreadCount { get; init; }
    public int ProcessId { get; init; }
    public TimeSpan Uptime { get; init; }
    public float CpuUsagePercent { get; init; }

    // GC metrics
    public int GcGen0Collections { get; init; }
    public int GcGen1Collections { get; init; }
    public int GcGen2Collections { get; init; }

    // Network metrics
    public int ActiveConnections { get; init; }
    public long TotalBytesReceived { get; init; }
    public long TotalBytesSent { get; init; }

    // Performance metrics
    public double AverageTickProcessingTime { get; init; }
    public int QueuedActions { get; init; }

    // Helper methods for human-readable values
    public string GetFormattedPrivateMemory() => PrivateMemoryBytes.Bytes().Humanize();
    public string GetFormattedPagedMemory() => PagedMemoryBytes.Bytes().Humanize();
    public string GetFormattedManagedMemory() => ManagedMemoryBytes.Bytes().Humanize();
    public string GetFormattedUptime() => Uptime.Humanize();

    // Constructor
    public DiagnosticMetrics(
        long privateMemoryBytes = 0,
        long pagedMemoryBytes = 0,
        long managedMemoryBytes = 0,
        int threadCount = 0,
        int processId = 0,
        TimeSpan uptime = default,
        float cpuUsagePercent = 0,
        int gcGen0Collections = 0,
        int gcGen1Collections = 0,
        int gcGen2Collections = 0,
        int activeConnections = 0,
        long totalBytesReceived = 0,
        long totalBytesSent = 0,
        double averageTickProcessingTime = 0,
        int queuedActions = 0
    )
    {
        PrivateMemoryBytes = privateMemoryBytes;
        PagedMemoryBytes = pagedMemoryBytes;
        ManagedMemoryBytes = managedMemoryBytes;
        ThreadCount = threadCount;
        ProcessId = processId;
        Uptime = uptime;
        CpuUsagePercent = cpuUsagePercent;
        GcGen0Collections = gcGen0Collections;
        GcGen1Collections = gcGen1Collections;
        GcGen2Collections = gcGen2Collections;
        ActiveConnections = activeConnections;
        TotalBytesReceived = totalBytesReceived;
        TotalBytesSent = totalBytesSent;
        AverageTickProcessingTime = averageTickProcessingTime;
        QueuedActions = queuedActions;
    }

    // Method to create a snapshot of current diagnostics
    public static DiagnosticMetrics CreateSnapshot(Process process)
    {
        return new DiagnosticMetrics(
            privateMemoryBytes: process.WorkingSet64,
            pagedMemoryBytes: process.PagedMemorySize64,
            managedMemoryBytes: GC.GetTotalMemory(false),
            threadCount: process.Threads.Count,
            processId: process.Id,
            uptime: GetUptime(process),
            cpuUsagePercent: 0, // This needs to be calculated separately
            gcGen0Collections: GC.CollectionCount(0),
            gcGen1Collections: GC.CollectionCount(1),
            gcGen2Collections: GC.CollectionCount(2)
        );
    }

    public static TimeSpan GetUptime(Process process) => DateTime.Now - process.StartTime;
}
