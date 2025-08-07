namespace AbyssIrc.Server.Core.Interfaces.Metrics;

/// <summary>
/// Interface for services that can provide metrics.
/// </summary>
public interface IMetricsProvider
{
    /// <summary>
    /// Gets the name of the service providing metrics.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the metrics object from this provider.
    /// </summary>
    /// <returns>An object containing metrics data.</returns>
    object GetMetrics();
}
