using AbyssIrc.Server.Core.Interfaces.Metrics;

namespace AbyssIrc.Server.Events.Diagnostic;

public record RegisterMetricEvent(IMetricsProvider provider);
