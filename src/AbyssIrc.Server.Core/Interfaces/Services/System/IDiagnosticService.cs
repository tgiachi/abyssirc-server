using AbyssIrc.Core.Interfaces.Services;
using AbyssIrc.Server.Core.Data.Metrics;

namespace AbyssIrc.Server.Core.Interfaces.Services.System;

public interface IDiagnosticService : IAbyssStarStopService
{
    // Get current metrics
    Task<DiagnosticMetrics> GetCurrentMetricsAsync();

    // Observable for continuous monitoring
    IObservable<DiagnosticMetrics> Metrics { get; }

    // Get the PID file path
    string PidFilePath { get; }

    // Force collect diagnostics now
    Task CollectMetricsAsync();
}
