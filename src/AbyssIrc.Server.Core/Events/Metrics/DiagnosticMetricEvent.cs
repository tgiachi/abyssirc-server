using AbyssIrc.Server.Core.Data.Metrics;

namespace AbyssIrc.Server.Core.Events.Metrics;

public record DiagnosticMetricEvent(DiagnosticMetrics Metrics);
