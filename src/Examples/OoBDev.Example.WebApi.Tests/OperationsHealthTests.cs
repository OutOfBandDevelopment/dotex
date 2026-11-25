using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class OperationsHealthTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Health_MixedChecks_ReportsWorstStatusAndEntriesSortedByName()
    {
        // Stage
        var report = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["sbert"] = new(HealthStatus.Healthy, "up", TimeSpan.FromMilliseconds(12.34), null, null),
                ["mailkit-smtp"] = new(HealthStatus.Unhealthy, "refused", TimeSpan.FromMilliseconds(3), null, null),
                ["ollama"] = new(HealthStatus.Degraded, null, TimeSpan.Zero, null, null),
            },
            TimeSpan.FromMilliseconds(20));
        var service = new Mock<HealthCheckService>();
        service.Setup(s => s.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>?>(), It.IsAny<CancellationToken>())).ReturnsAsync(report);
        var controller = new OperationsController(new ConfigurationBuilder().Build(), Mock.Of<IWebHostEnvironment>(), service.Object);

        // Test
        var result = await controller.Health(TestContext.CancellationToken);
        TestContext.WriteLine($"status={result.Status} entries={string.Join(',', result.Entries.Select(e => $"{e.Name}:{e.Status}"))}");

        // Assert
        Assert.AreEqual("Unhealthy", result.Status);
        CollectionAssert.AreEqual(new[] { "mailkit-smtp", "ollama", "sbert" }, result.Entries.Select(e => e.Name).ToArray());
        Assert.AreEqual(12.3, result.Entries[2].DurationMs);
        Assert.AreEqual("refused", result.Entries[0].Description);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Health_NoChecksRegistered_IsHealthyWithNoEntries()
    {
        // Stage
        var service = new Mock<HealthCheckService>();
        service.Setup(s => s.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero));
        var controller = new OperationsController(new ConfigurationBuilder().Build(), Mock.Of<IWebHostEnvironment>(), service.Object);

        // Test
        var result = await controller.Health(TestContext.CancellationToken);

        // Assert
        Assert.AreEqual("Healthy", result.Status);
        Assert.IsEmpty(result.Entries);
    }
}
