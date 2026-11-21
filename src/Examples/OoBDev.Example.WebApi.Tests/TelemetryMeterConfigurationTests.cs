using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Middleware;
using OoBDev.TestUtilities;
using System;
using System.IO;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class TelemetryMeterConfigurationTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AppSettings_Meters_ListTheUsageAndScreenViewMeters()
    {
        // Stage: the shipped settings of the example API
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "OoBDev.Example.WebApi", "appsettings.json")))
            dir = dir.Parent;
        Assert.IsNotNull(dir, "OoBDev.Example.WebApi/appsettings.json not found above the test output.");
        var file = Path.Combine(dir.FullName, "OoBDev.Example.WebApi", "appsettings.json");
        TestContext.WriteLine($"Reading {file}");

        // Test
        var meters = new ConfigurationBuilder().AddJsonFile(file).Build().GetSection("OpenTelemetryOptions:Meters").Get<string[]>() ?? [];
        TestContext.WriteLine($"Configured meters: {string.Join(", ", meters)}");

        // Assert: an unlisted meter is never exported, so the dashboard would show no usage metrics
        CollectionAssert.IsSubsetOf(
            new[] { HttpUsageMetricsMiddleware.MeterName, ScreenViewEndpointRouteBuilderExtensions.MeterName },
            meters);
    }
}
