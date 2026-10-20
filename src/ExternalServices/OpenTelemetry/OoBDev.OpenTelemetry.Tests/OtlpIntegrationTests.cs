using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using OoBDev.TestUtilities;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.OpenTelemetry.Tests;

/// <summary>
/// Sends spans and logs over OTLP to the Aspire dashboard container and reads them back from its telemetry API.
/// </summary>
[TestClass]
public partial class OtlpIntegrationTests
{
    private const string SourceName = "OoBDev.Tests.OtlpIntegration";

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Spans_ShouldReachDashboardWithCorrelationTags()
    {
        // Stage
        var serviceName = $"oobdev-it-{Guid.NewGuid():N}";
        var correlationId = $"corr-{Guid.NewGuid():N}";
        using var provider = CreateProvider(serviceName, correlationId);
        var tracer = provider.GetRequiredService<TracerProvider>();
        string traceId;

        // Test
        using (var source = new ActivitySource(SourceName))
        using (var activity = source.StartActivity("integration-work"))
        {
            Assert.IsNotNull(activity);
            traceId = activity.TraceId.ToString();
        }
        Assert.IsTrue(tracer.ForceFlush(10_000));

        // Assert
        var trace = await PollAsync($"/api/telemetry/spans?traceId={traceId}", body => body.Contains(correlationId));
        Assert.IsNotNull(trace, "The trace with the correlation tag did not arrive in the Aspire dashboard.");
        StringAssert.Contains(trace, serviceName);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Logs_ShouldReachDashboardWithCorrelationAttributes()
    {
        // Stage
        var serviceName = $"oobdev-it-{Guid.NewGuid():N}";
        var correlationId = $"corr-{Guid.NewGuid():N}";
        using var provider = CreateProvider(serviceName, correlationId);
        var logs = provider.GetRequiredService<LoggerProvider>();

        // Test
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("integration");
        LogHello(logger, "otlp");
        Assert.IsTrue(logs.ForceFlush(10_000));

        // Assert
        var result = await PollAsync(
            $"/api/telemetry/logs?resource={Uri.EscapeDataString(serviceName)}",
            body => body.Contains(correlationId));
        Assert.IsNotNull(result, "The log record with the correlation attribute did not arrive in the Aspire dashboard.");
        StringAssert.Contains(result, "Hello {Name}");
    }

    private ServiceProvider CreateProvider(string serviceName, string correlationId)
    {
        var endpoint = TestContext.GetRequiredProperty<string>("OTEL_EXPORTER_OTLP_ENDPOINT");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(OpenTelemetryOptions)}:OtlpEndpoint"] = endpoint,
                [$"{nameof(OpenTelemetryOptions)}:ServiceName"] = serviceName,
                [$"{nameof(OpenTelemetryOptions)}:Sources:0"] = SourceName,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IAccessor<CorrelationInfo>>(
            new TestCorrelationAccessor(new CorrelationInfo { CorrelationId = correlationId }));
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());
        services.TryAddOpenTelemetryExtensions(configuration);
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Calls the dashboard telemetry API until the body satisfies <paramref name="accept"/>; returns null if it never does.
    /// </summary>
    private async Task<string?> PollAsync(string path, Func<string, bool> accept)
    {
        var dashboard = TestContext.GetRequiredProperty<string>("OTEL_DASHBOARD_URL");
        using var client = new HttpClient { BaseAddress = new Uri(dashboard) };
        client.DefaultRequestHeaders.Add("x-api-key", TestContext.GetPropertyOrDefault("OTEL_DASHBOARD_API_KEY", "oobdev-test-key"));
        for (var attempt = 0; attempt < 30; attempt++)
        {
            using var response = await client.GetAsync(path, TestContext.CancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);
                if (accept(body)) return body;
            }
            await Task.Delay(TimeSpan.FromSeconds(1), TestContext.CancellationToken);
        }
        return null;
    }

    private sealed class TestCorrelationAccessor(CorrelationInfo value) : IAccessor<CorrelationInfo>
    {
        public CorrelationInfo? Value { get; set; } = value;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Hello {Name}")]
    private static partial void LogHello(ILogger logger, string name);
}
