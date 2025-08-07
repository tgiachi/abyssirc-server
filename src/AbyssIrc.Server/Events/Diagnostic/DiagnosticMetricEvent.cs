using AbyssIrc.Server.Core.Data.Metrics.Diagnostic;

namespace AbyssIrc.Server.Events.Diagnostic;

public record DiagnosticMetricEvent(MetricProviderData Metrics);
