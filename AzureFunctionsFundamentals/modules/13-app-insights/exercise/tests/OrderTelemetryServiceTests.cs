using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsFundamentals.Modules.AppInsights.Exercise;
using AzureFunctionsFundamentals.Shared;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AzureFunctionsFundamentals.Modules.AppInsights.Exercise.Tests;

internal sealed class FakeTelemetryChannel : IDisposable
{
    private readonly ConcurrentBag<ITelemetry> _items = [];
    private readonly ActivityListener _activityListener;
    private readonly MeterListener _meterListener;

    public IReadOnlyList<ITelemetry> Items => _items.ToList().AsReadOnly();

    public FakeTelemetryChannel(TelemetryConfiguration config)
    {
        // 1. Hook up ActivityListener to intercept dependencies
        _activityListener = new ActivityListener
        {
            ShouldListenTo = (source) => source.Name == "Microsoft.ApplicationInsights",
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStopped = (activity) =>
            {
                var name = activity.Tags.FirstOrDefault(t => t.Key == "microsoft.dependency.name").Value ?? activity.DisplayName;
                var type = activity.Tags.FirstOrDefault(t => t.Key == "microsoft.dependency.type").Value ?? "Dependency";
                var data = activity.Tags.FirstOrDefault(t => t.Key == "microsoft.dependency.data").Value;
                var success = activity.Status != ActivityStatusCode.Error;

                var dep = new DependencyTelemetry
                {
                    Name = name,
                    Type = type,
                    Data = data,
                    Success = success,
                    Duration = activity.Duration
                };
                _items.Add(dep);
            }
        };
        ActivitySource.AddActivityListener(_activityListener);

        // 2. Hook up MeterListener to intercept metrics
        _meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "Microsoft.ApplicationInsights")
                {
                    listener.EnableMeasurementEvents(instrument, null);
                }
            }
        };
        _meterListener.SetMeasurementEventCallback<double>((instrument, value, tags, state) =>
        {
            var metric = new MetricTelemetry(instrument.Name, value);
            _items.Add(metric);
        });
        _meterListener.SetMeasurementEventCallback<int>((instrument, value, tags, state) =>
        {
            var metric = new MetricTelemetry(instrument.Name, value);
            _items.Add(metric);
        });
        _meterListener.SetMeasurementEventCallback<long>((instrument, value, tags, state) =>
        {
            var metric = new MetricTelemetry(instrument.Name, value);
            _items.Add(metric);
        });
        _meterListener.Start();

        // 3. Hook up OpenTelemetry logger provider to intercept custom events
        config.ConfigureOpenTelemetryBuilder(builder =>
        {
            builder.Services.AddSingleton<ILoggerProvider>(new FakeEventLoggerProvider(this));
        });
    }

    public void AddEvent(EventTelemetry ev)
    {
        _items.Add(ev);
    }

    public void Dispose()
    {
        _activityListener.Dispose();
        _meterListener.Dispose();
    }

    private class FakeEventLoggerProvider(FakeTelemetryChannel channel) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new FakeEventLogger(channel);
        public void Dispose() { }
    }

    private class FakeEventLogger(FakeTelemetryChannel channel) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (state is IEnumerable<KeyValuePair<string, object>> kvps)
            {
                var dict = kvps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty);
                if (dict.TryGetValue("microsoft.custom_event.name", out var eventName))
                {
                    var ev = new EventTelemetry { Name = eventName };
                    foreach (var kvp in dict)
                    {
                        if (kvp.Key != "microsoft.custom_event.name" && kvp.Key != "{OriginalFormat}")
                        {
                            ev.Properties[kvp.Key] = kvp.Value;
                        }
                    }
                    channel.AddEvent(ev);
                }
            }
        }
    }
}

internal sealed class FakeLogEntry
{
    public LogLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public Exception? Exception { get; init; }
    public IReadOnlyDictionary<string, object?> Scope { get; init; } = new Dictionary<string, object?>();
}

internal sealed class FakeLogger<T> : ILogger<T>
{
    private readonly List<FakeLogEntry> _entries = [];
    private readonly Stack<IReadOnlyDictionary<string, object?>> _scopeStack = new();

    public IReadOnlyList<FakeLogEntry> Entries => _entries.AsReadOnly();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        var dict = state as IReadOnlyDictionary<string, object?>
            ?? (state as IEnumerable<KeyValuePair<string, object?>>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ?? new Dictionary<string, object?> { ["state"] = state };
        _scopeStack.Push(dict);
        return new ScopeHandle(() => _scopeStack.TryPop(out _));
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var scope = _scopeStack.Count > 0 ? _scopeStack.Peek() : new Dictionary<string, object?>();
        _entries.Add(new FakeLogEntry
        {
            Level = logLevel,
            Message = formatter(state, exception),
            Exception = exception,
            Scope = scope
        });
    }

    private sealed class ScopeHandle(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }
}

public sealed class OrderTelemetryServiceTests : IDisposable
{
    private readonly FakeTelemetryChannel _channel;
    private readonly TelemetryClient _telemetry;
    private readonly FakeLogger<OrderTelemetryService> _logger = new();
    private readonly OrderTelemetryService _service;

    public OrderTelemetryServiceTests()
    {
        var config = new TelemetryConfiguration();
        config.ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000";
        _channel = new FakeTelemetryChannel(config);
        _telemetry = new TelemetryClient(config);
        _service = new OrderTelemetryService(_logger, _telemetry);
    }

    public void Dispose()
    {
        _channel.Dispose();
    }

    [Fact]
    public async Task ProcessAsync_ValidOrder_LogsStartAndCompletion()
    {
        var order = new Order { Id = "ord-1", CustomerId = 42, Product = "Keyboard", Quantity = 2, UnitPrice = 50m };

        var result = await _service.ProcessAsync(order);

        Assert.True(result.IsSuccess);
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("ord-1"));
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Information && e.Message.Length > 0);
    }

    [Fact]
    public async Task ProcessAsync_ValidOrder_AttachesOrderIdToScope()
    {
        var order = new Order { Id = "ord-2", CustomerId = 7, Product = "Mouse", Quantity = 1, UnitPrice = 25m };

        await _service.ProcessAsync(order);

        var scopedEntries = _logger.Entries.Where(e =>
            e.Scope.ContainsKey("OrderId") || e.Scope.ContainsKey("orderId")).ToList();

        Assert.NotEmpty(scopedEntries);
    }

    [Fact]
    public async Task ProcessAsync_ValidOrder_TracksOrderProcessedEvent()
    {
        var order = new Order { Id = "ord-3", CustomerId = 10, Product = "Monitor", Quantity = 1, UnitPrice = 300m };

        await _service.ProcessAsync(order);

        _telemetry.Flush();
        var events = _channel.Items.OfType<EventTelemetry>().ToList();
        Assert.Contains(events, e => e.Name == "OrderProcessed");

        var ev = events.First(e => e.Name == "OrderProcessed");
        Assert.True(ev.Properties.ContainsKey("OrderId"));
        Assert.Equal("ord-3", ev.Properties["OrderId"]);
    }

    [Fact]
    public async Task ProcessAsync_ValidOrder_TracksOrderValueMetric()
    {
        var order = new Order { Id = "ord-4", CustomerId = 5, Product = "Tablet", Quantity = 2, UnitPrice = 199m };

        await _service.ProcessAsync(order);

        _telemetry.Flush();
        var metrics = _channel.Items.OfType<MetricTelemetry>().ToList();
        Assert.Contains(metrics, m => m.Name == "OrderValue");

        var metric = metrics.First(m => m.Name == "OrderValue");
        Assert.Equal((double)order.Total, metric.Value);
    }

    [Fact]
    public async Task ProcessAsync_NullOrder_ReturnsFailureAndLogsWarning()
    {
        var result = await _service.ProcessAsync(null);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task ProcessAsync_InvalidCustomerId_ReturnsFailureAndLogsWarning()
    {
        var order = new Order { Id = "ord-5", CustomerId = 0, Product = "Widget", Quantity = 1, UnitPrice = 10m };

        var result = await _service.ProcessAsync(order);

        Assert.False(result.IsSuccess);
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task TrackOrderDependency_OnSuccess_TracksDependencyWithSuccess()
    {
        var called = false;

        await _service.TrackOrderDependency("SaveOrder", "dbo.Orders", () =>
        {
            called = true;
            return Task.CompletedTask;
        });

        Assert.True(called);
        _telemetry.Flush();
        var deps = _channel.Items.OfType<DependencyTelemetry>().ToList();
        Assert.Contains(deps, d => d.Name == "SaveOrder" && d.Success == true);
    }

    [Fact]
    public async Task TrackOrderDependency_OnException_TracksDependencyWithFailureAndRethrows()
    {
        var ex = new InvalidOperationException("DB error");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.TrackOrderDependency("SaveOrder", "dbo.Orders", () => throw ex));

        _telemetry.Flush();
        var deps = _channel.Items.OfType<DependencyTelemetry>().ToList();
        Assert.Contains(deps, d => d.Name == "SaveOrder" && d.Success == false);
    }
}
