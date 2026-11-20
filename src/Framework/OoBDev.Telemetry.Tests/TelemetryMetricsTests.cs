using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace OoBDev.Telemetry.Tests;

[TestClass]
public class TelemetryMetricsTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddOpenTelemetryExtensions_ConfiguredMeter_IsCollected()
    {
        // Stage
        var exported = new List<Metric>();
        var services = new ServiceCollection();
        services.TryAddOpenTelemetryExtensions(new OpenTelemetryOptions { Meters = ["OoBDev.Tests.Metrics"] }, exportOtlp: false);
        services.ConfigureOpenTelemetryMeterProvider(b => b.AddInMemoryExporter(exported));
        using var provider = services.BuildServiceProvider();
        using var meterProvider = provider.GetRequiredService<MeterProvider>();
        using var meter = new Meter("OoBDev.Tests.Metrics");
        using var other = new Meter("OoBDev.Tests.Other");

        // Test
        meter.CreateCounter<long>("seen").Add(3);
        other.CreateCounter<long>("ignored").Add(1);
        meterProvider.ForceFlush();

        // Assert
        TestContext.WriteLine(string.Join(", ", exported.Select(m => m.Name)));
        Assert.AreEqual("seen", Assert.ContainsSingle(exported).Name);
    }
}
