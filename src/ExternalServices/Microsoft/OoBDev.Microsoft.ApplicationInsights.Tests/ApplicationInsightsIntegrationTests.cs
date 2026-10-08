using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.ApplicationInsights.Tests;

/// <summary>
/// Integration tests for Application Insights 3.x (OpenTelemetry) using the azurinsight emulator.
/// Tests verify that spans, logs and metrics are sent to and stored in azurinsight.
/// </summary>
[TestClass]
public class ApplicationInsightsIntegrationTests
{
    private const string SourceName = "OoBDev.Tests.ApplicationInsights";

    private ServiceProvider? _serviceProvider;
    private HttpClient? _httpClient;

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    /// </summary>
    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        var connectionString = TestContext.GetRequiredProperty<string>("APPINSIGHTS_CONNECTION_STRING");
        var azurinsightUrl = TestContext.GetRequiredProperty<string>("APPINSIGHTS_URL");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOpenTelemetry()
            .WithTracing(t => t.AddSource(SourceName))
            .WithMetrics(m => m.AddMeter(SourceName));
        services.AddApplicationInsightsTelemetry(o => o.ConnectionString = connectionString);
        _serviceProvider = services.BuildServiceProvider();

        // Ensure the SDK providers are created before telemetry is produced.
        _ = _serviceProvider.GetRequiredService<TracerProvider>();
        _ = _serviceProvider.GetRequiredService<MeterProvider>();

        _httpClient = new HttpClient { BaseAddress = new Uri(azurinsightUrl) };
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        Flush();
        await Task.Delay(1000);

        try
        {
            if (_httpClient != null)
            {
                var response = await _httpClient.PostAsync("/api/purge", null);
                response.EnsureSuccessStatusCode();
            }
        }
        catch
        {
            // Ignore cleanup errors
        }

        _httpClient?.Dispose();
        _serviceProvider?.Dispose();
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendSpan_ShouldStoreInAzurinsight()
    {
        // Stage
        var spanName = "TestSpan-" + Guid.NewGuid().ToString("N");

        // Test
        using (var source = new ActivitySource(SourceName))
        using (var activity = source.StartActivity(spanName))
        {
            activity?.SetTag("Property1", "Value1");
        }

        // Assert
        var content = await QueryAsync();
        Assert.Contains(spanName, content, $"Span '{spanName}' not found in azurinsight");
        Assert.Contains("Value1", content, "Value1 not found in telemetry");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendLog_ShouldStoreInAzurinsight()
    {
        // Stage
        var message = "Test trace message " + Guid.NewGuid().ToString("N");

        // Test
        _serviceProvider!.GetRequiredService<ILoggerFactory>().CreateLogger("test").LogInformation("{Message}", message);

        // Assert
        var content = await QueryAsync();
        Assert.Contains(message, content, $"Log message '{message}' not found in azurinsight");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendMetric_ShouldStoreInAzurinsight()
    {
        // Stage
        var name = "test.counter." + Guid.NewGuid().ToString("N");

        // Test
        using (var meter = new Meter(SourceName))
        {
            meter.CreateCounter<long>(name).Add(5);
            _serviceProvider!.GetRequiredService<MeterProvider>().ForceFlush();
        }

        // Assert
        var content = await QueryAsync();
        Assert.Contains(name, content, $"Metric '{name}' not found in azurinsight");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendException_ShouldStoreInAzurinsight()
    {
        // Stage
        var message = "Test exception " + Guid.NewGuid().ToString("N");

        // Test
        _serviceProvider!.GetRequiredService<ILoggerFactory>().CreateLogger("test")
            .LogError(new InvalidOperationException(message), "failed");

        // Assert
        var content = await QueryAsync();
        Assert.Contains(message, content, $"Exception '{message}' not found in azurinsight");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task PurgeApi_ShouldClearAllTelemetry()
    {
        // Stage
        var spanName = "PurgeSpan-" + Guid.NewGuid().ToString("N");
        using (var source = new ActivitySource(SourceName))
        using (source.StartActivity(spanName))
        {
        }
        Assert.Contains(spanName, await QueryAsync());

        // Test
        var purge = await _httpClient!.PostAsync("/api/purge", null);
        purge.EnsureSuccessStatusCode();

        // Assert
        Assert.DoesNotContain(spanName, await QueryAsync());
    }

    private void Flush()
    {
        _serviceProvider?.GetService<TracerProvider>()?.ForceFlush();
        _serviceProvider?.GetService<MeterProvider>()?.ForceFlush();
    }

    private async Task<string> QueryAsync()
    {
        Flush();
        await Task.Delay(2000);
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
