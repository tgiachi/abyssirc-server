using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AbyssIrc.Server.Core.Data.Metrics.Diagnostic;
using AbyssIrc.Server.Core.Data.Services;
using AbyssIrc.Server.Core.Directories;
using AbyssIrc.Server.Core.Interfaces.Metrics;
using AbyssIrc.Server.Core.Interfaces.Services;
using AbyssIrc.Server.Events.Diagnostic;
using Serilog;

namespace AbyssIrc.Server.Services;

public class DiagnosticService : IDiagnosticService, IMetricsProvider
{
    public string ProviderName => "SystemMetrics";
    public string PidFilePath { get; }

    private readonly ILogger _logger = Log.ForContext<DiagnosticService>();

    private readonly IEventBusService _eventBusService;
    private readonly DiagnosticServiceConfig _diagnosticServiceConfig;

    private readonly ISchedulerSystemService _schedulerService;
    private readonly Subject<MetricProviderData> _metricsSubject = new();
    private long _uptimeStopwatch;
    private readonly Process _currentProcess;


    private readonly Dictionary<string, IMetricsProvider> _metricsProviders = new();

    private int _lastGcGen0;
    private int _lastGcGen1;
    private int _lastGcGen2;

    public Task<List<MetricProviderData>> GetCurrentMetricsAsync()
    {
        var metrics = GetAllProvidersMetrics();
        var metricList = new List<MetricProviderData>();

        foreach (var kvp in metrics)
        {
            if (kvp.Value is MetricProviderData metricData)
            {
                metricList.Add(metricData);
            }
        }

        return Task.FromResult(metricList);
    }

    public IObservable<MetricProviderData> Metrics => _metricsSubject.AsObservable();


    public DiagnosticService(
        ISchedulerSystemService schedulerService, DirectoriesConfig directoriesConfig,
        IEventBusService eventBusService,
        DiagnosticServiceConfig diagnosticServiceConfig
    )
    {
        _schedulerService = schedulerService;
        _eventBusService = eventBusService;
        _diagnosticServiceConfig = diagnosticServiceConfig;


        _eventBusService.Subscribe<RegisterMetricEvent>(OnRegisterMetricEvent);
        PidFilePath = Path.Combine(directoriesConfig.Root, _diagnosticServiceConfig.PidFileName);
        _currentProcess = Process.GetCurrentProcess();

        // Initialize GC collection counts
        _lastGcGen0 = GC.CollectionCount(0);
        _lastGcGen1 = GC.CollectionCount(1);
        _lastGcGen2 = GC.CollectionCount(2);

        RegisterMetricsProvider(this);
    }

    private async Task OnRegisterMetricEvent(RegisterMetricEvent @event)
    {
        RegisterMetricsProvider(@event.provider);
    }

    public object GetMetrics()
    {
        return CollectMetricsInternalAsync();
    }


    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _uptimeStopwatch = Stopwatch.GetTimestamp();
        WritePidFile();


        // Schedule regular metrics collection
        await _schedulerService.RegisterJob(
            "diagnostic_metrics",
            CollectMetricsAsync,
            TimeSpan.FromSeconds(_diagnosticServiceConfig.MetricsIntervalInSeconds)
        );

        var fileName = Path.GetFileName(_diagnosticServiceConfig.PidFileName);

        _logger.Information("Diagnostic service started. PID: {Pid} in file: {FileName}", _currentProcess.Id, fileName);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        DeletePidFile();
        _metricsSubject.Dispose();
        _logger.Information("Diagnostic service stopped");
    }


    public async Task CollectMetricsAsync()
    {
        foreach (var provider in _metricsProviders)
        {
            var metric = provider.Value.GetMetrics();

            var metrics = new MetricProviderData(
                provider.Key,
                metric
            );
            _metricsSubject.OnNext(metrics);
            await _eventBusService.PublishAsync(new DiagnosticMetricEvent(metrics));

            _logger.Debug("[METRICS] {ProviderName}: {Metrics}",
                provider.Key, metric
            );;
        }
    }

    public void RegisterMetricsProvider(IMetricsProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (!_metricsProviders.TryAdd(provider.ProviderName, provider))
        {
            throw new InvalidOperationException(
                $"Metrics provider with name {provider.ProviderName} is already registered."
            );
        }
    }

    public void UnregisterMetricsProvider(string providerName)
    {
        if (!_metricsProviders.Remove(providerName))
        {
            throw new InvalidOperationException($"Metrics provider with name {providerName} is not registered.");
        }

        _logger.Debug("Unregistered metrics provider: {ProviderName}", providerName);
    }

    public Dictionary<string, object> GetAllProvidersMetrics()
    {
        return _metricsProviders
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetMetrics()
            );
    }

    private DiagnosticMetrics CollectMetricsInternalAsync()
    {
        var currentGen0 = GC.CollectionCount(0);
        var currentGen1 = GC.CollectionCount(1);
        var currentGen2 = GC.CollectionCount(2);

        var metrics = new DiagnosticMetrics(
            privateMemoryBytes: _currentProcess.WorkingSet64,
            pagedMemoryBytes: GC.GetTotalMemory(false),
            threadCount: _currentProcess.Threads.Count,
            processId: _currentProcess.Id,
            uptime: Stopwatch.GetElapsedTime(_uptimeStopwatch),
            cpuUsagePercent: 0,
            gcGen0Collections: currentGen0 - _lastGcGen0,
            gcGen1Collections: currentGen1 - _lastGcGen1,
            gcGen2Collections: currentGen2 - _lastGcGen2
        );

        // Update last GC collection counts
        _lastGcGen0 = currentGen0;
        _lastGcGen1 = currentGen1;
        _lastGcGen2 = currentGen2;

        return metrics;
    }

    private void WritePidFile()
    {
        try
        {
            DeletePidFile();

            File.WriteAllText(PidFilePath, _currentProcess.Id.ToString());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write PID file");
        }
    }

    private void DeletePidFile()
    {
        try
        {
            if (File.Exists(PidFilePath))
            {
                File.Delete(PidFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete PID file");
        }
    }

    public void Dispose()
    {
        _metricsSubject.Dispose();
        _currentProcess.Dispose();
    }
}
