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
/// Sends spans and logs over OTLP to the Grafana LGTM container and reads them back from Tempo and Loki.
/// </summary>
[TestClass]
public class OtlpIntegrationTests
{
    private const string SourceName = "OoBDev.Tests.OtlpIntegration";

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Spans_ShouldReachTempoWithCorrelationTags()
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
        var trace = await PollAsync($"/api/datasources/proxy/uid/tempo/api/traces/{traceId}", body => body.Contains(correlationId));
        Assert.IsNotNull(trace, "The trace with the correlation tag did not arrive in Tempo.");
        StringAssert.Contains(trace, serviceName);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Logs_ShouldReachLokiWithCorrelationAttributes()
    {
        // Stage
        var serviceName = $"oobdev-it-{Guid.NewGuid():N}";
        var correlationId = $"corr-{Guid.NewGuid():N}";
        using var provider = CreateProvider(serviceName, correlationId);
        var logs = provider.GetRequiredService<LoggerProvider>();

        // Test
        provider.GetRequiredService<ILoggerFactory>().CreateLogger("integration").LogInformation("Hello {Name}", "otlp");
        Assert.IsTrue(logs.ForceFlush(10_000));

        // Assert
        var query = Uri.EscapeDataString($"{{service_name=\"{serviceName}\"}}");
        var result = await PollAsync(
            $"/api/datasources/proxy/uid/loki/loki/api/v1/query_range?query={query}&limit=10",
            body => body.Contains(correlationId));
        Assert.IsNotNull(result, "The log record with the correlation attribute did not arrive in Loki.");
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
    /// Calls the Grafana proxy until the body satisfies <paramref name="accept"/>; returns null if it never does.
    /// </summary>
    private async Task<string?> PollAsync(string path, Func<string, bool> accept)
    {
        var grafana = TestContext.GetRequiredProperty<string>("OTEL_GRAFANA_URL");
        using var client = new HttpClient { BaseAddress = new Uri(grafana) };
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
}
