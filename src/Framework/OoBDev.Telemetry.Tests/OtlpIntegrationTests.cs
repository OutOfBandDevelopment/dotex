using OoBDev.System.Security;
using OoBDev.TestUtilities.Containers;
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

namespace OoBDev.Telemetry.Tests;

/// <summary>
/// Sends spans and logs over OTLP to the Aspire dashboard container and reads them back from its telemetry API.
/// </summary>
[TestClass]
public partial class OtlpIntegrationTests
{
    private const string SourceName = "OoBDev.Tests.OtlpIntegration";

    public required TestContext TestContext { get; set; }

    [IntegrationTestMethod("oobd-test-otel")]
    [TestCategory(TestCategories.Integration)]
    public async Task Spans_ShouldReachDashboardWithCorrelationTags()
    {
        // Stage
        await WaitForDashboardAsync();
        var serviceName = $"oobdev-it-{Guid.NewGuid():N}";
        var correlationId = $"corr-{Guid.NewGuid():N}";
        using var provider = CreateProvider(serviceName, correlationId);
        var tracer = provider.GetRequiredService<TracerProvider>();
        string traceId;

        // Test
        TestContext.WriteLine($"Test: span in service {serviceName}, correlation {correlationId}");
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

    [IntegrationTestMethod("oobd-test-otel")]
    [TestCategory(TestCategories.Integration)]
    public async Task Logs_ShouldReachDashboardWithCorrelationAttributes()
    {
        // Stage
        var serviceName = $"oobdev-it-{Guid.NewGuid():N}";
        var correlationId = $"corr-{Guid.NewGuid():N}";
        using var provider = CreateProvider(serviceName, correlationId);
        var logs = provider.GetRequiredService<LoggerProvider>();

        // Test
        TestContext.WriteLine($"Test: log record in service {serviceName}, correlation {correlationId}");
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
        services.AddSingleton<ICurrentUserAccessor>(new EnvironmentUserAccessor());
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
        for (var attempt = 1; attempt <= 30; attempt++)
        {
            try
            {
                using var response = await client.GetAsync(path, TestContext.CancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);
                    if (accept(body))
                    {
                        TestContext.WriteLine($"Poll {path}: accepted on attempt {attempt}");
                        return body;
                    }
                }
                else
                {
                    TestContext.WriteLine($"Poll {path}: attempt {attempt} returned {(int)response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // The container is running before the dashboard listens: connections are reset until then.
                TestContext.WriteLine($"Poll {path}: attempt {attempt} not reachable yet ({ex.InnerException?.Message ?? ex.Message})");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), TestContext.CancellationToken);
        }

        return null;
    }

    /// <summary>The Aspire image has no health check, so wait for its telemetry API before exporting anything to it.</summary>
    private async Task WaitForDashboardAsync()
    {
        TestContext.WriteLine("Stage: waiting for the Aspire dashboard telemetry API");
        if (await PollAsync("/api/telemetry/spans", _ => true) is null)
            Assert.Inconclusive("The Aspire dashboard telemetry API did not answer within 30 s.");
    }

    private sealed class TestCorrelationAccessor(CorrelationInfo value) : IAccessor<CorrelationInfo>
    {
        public CorrelationInfo? Value { get; set; } = value;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Hello {Name}")]
    private static partial void LogHello(ILogger logger, string name);
}
