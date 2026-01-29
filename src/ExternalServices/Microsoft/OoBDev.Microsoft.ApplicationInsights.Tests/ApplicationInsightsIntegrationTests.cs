using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.ApplicationInsights.Tests;

/// <summary>
/// Integration tests for Application Insights telemetry using azurinsight emulator.
/// Tests verify that telemetry is correctly sent to and stored in azurinsight.
/// </summary>
[TestClass]
public class ApplicationInsightsIntegrationTests
{
    private TelemetryClient? _telemetryClient;
    private TelemetryConfiguration? _configuration;
    private HttpClient? _httpClient;

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    /// </summary>
    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        // Get connection string from test context
        var connectionString = TestContext.GetRequiredProperty<string>("APPINSIGHTS_CONNECTION_STRING");
        var azurinsightUrl = TestContext.GetRequiredProperty<string>("APPINSIGHTS_URL");

        // Configure Application Insights to use azurinsight emulator
        _configuration = TelemetryConfiguration.CreateDefault();
        _configuration.ConnectionString = connectionString;

        // Use InMemoryChannel for immediate transmission to emulator (important for testing)
        _configuration.TelemetryChannel = new InMemoryChannel
        {
            EndpointAddress = azurinsightUrl + "/v2.1/track"
        };

        _telemetryClient = new TelemetryClient(_configuration);

        // HTTP client for querying azurinsight API
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(azurinsightUrl)
        };
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        // Flush all telemetry before cleanup
        _telemetryClient?.Flush();
        await Task.Delay(1000); // Give time for telemetry to be sent

        // Purge telemetry data from azurinsight
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

        _configuration?.Dispose();
        _httpClient?.Dispose();
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendEventTelemetry_ShouldStoreInAzurinsight()
    {
        // Arrange
        var eventName = "TestEvent";
        var properties = new Dictionary<string, string>
        {
            { "Property1", "Value1" },
            { "Property2", "Value2" }
        };
        var metrics = new Dictionary<string, double>
        {
            { "Metric1", 123.45 }
        };

        // Act
        _telemetryClient!.TrackEvent(eventName, properties, metrics);
        _telemetryClient.Flush();
        await Task.Delay(2000); // Wait for telemetry to be processed

        // Assert - Query azurinsight to verify telemetry was received
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(eventName, content, $"Event '{eventName}' not found in azurinsight");
        Assert.Contains("Property1", content, "Property1 not found in telemetry");
        Assert.Contains("Value1", content, "Value1 not found in telemetry");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendTraceTelemetry_ShouldStoreInAzurinsight()
    {
        // Arrange
        var traceMessage = "Test trace message";
        var severityLevel = SeverityLevel.Information;

        // Act
        _telemetryClient!.TrackTrace(traceMessage, severityLevel);
        _telemetryClient.Flush();
        await Task.Delay(2000);

        // Assert
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(traceMessage, content, $"Trace message '{traceMessage}' not found in azurinsight");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendMetricTelemetry_ShouldStoreInAzurinsight()
    {
        // Arrange
        var metricName = "TestMetric";
        var metricValue = 42.0;

        // Act
        _telemetryClient!.TrackMetric(metricName, metricValue);
        _telemetryClient.Flush();
        await Task.Delay(2000);

        // Assert
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(metricName, content, $"Metric '{metricName}' not found in azurinsight");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendExceptionTelemetry_ShouldStoreInAzurinsight()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception message");
        var properties = new Dictionary<string, string>
        {
            { "ErrorCode", "TEST001" }
        };

        // Act
        _telemetryClient!.TrackException(exception, properties);
        _telemetryClient.Flush();
        await Task.Delay(2000);

        // Assert
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test exception message", content, "Exception message not found in azurinsight");
        Assert.Contains("InvalidOperationException", content, "Exception type not found in azurinsight");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendDependencyTelemetry_ShouldStoreInAzurinsight()
    {
        // Arrange
        var dependencyName = "TestDependency";
        var dependencyType = "HTTP";
        var data = "GET https://api.example.com/data";
        var startTime = DateTimeOffset.UtcNow;
        var duration = TimeSpan.FromMilliseconds(150);
        var success = true;

        // Act
        _telemetryClient!.TrackDependency(dependencyType, dependencyName, data, startTime, duration, success);
        _telemetryClient.Flush();
        await Task.Delay(2000);

        // Assert
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(dependencyName, content, $"Dependency '{dependencyName}' not found in azurinsight");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendRequestTelemetry_ShouldStoreInAzurinsight()
    {
        // Arrange
        var requestName = "GET /api/test";
        var startTime = DateTimeOffset.UtcNow;
        var duration = TimeSpan.FromMilliseconds(50);
        var responseCode = "200";
        var success = true;

        var requestTelemetry = new RequestTelemetry
        {
            Name = requestName,
            Timestamp = startTime,
            Duration = duration,
            ResponseCode = responseCode,
            Success = success,
            Url = new Uri("https://localhost/api/test")
        };

        // Act
        _telemetryClient!.TrackRequest(requestTelemetry);
        _telemetryClient.Flush();
        await Task.Delay(2000);

        // Assert
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(requestName, content, $"Request '{requestName}' not found in azurinsight");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task PurgeApi_ShouldClearAllTelemetry()
    {
        // Arrange - Send some telemetry
        _telemetryClient!.TrackEvent("EventBeforePurge");
        _telemetryClient.Flush();
        await Task.Delay(2000);

        // Verify telemetry exists
        var beforePurge = await _httpClient!.GetAsync("/api/query");
        var beforeContent = await beforePurge.Content.ReadAsStringAsync();
        Assert.Contains("EventBeforePurge", beforeContent, "Event should exist before purge");

        // Act - Purge all telemetry
        var purgeResponse = await _httpClient.PostAsync("/api/purge", null);
        purgeResponse.EnsureSuccessStatusCode();
        await Task.Delay(1000);

        // Assert - Verify telemetry is cleared
        var afterPurge = await _httpClient.GetAsync("/api/query");
        var afterContent = await afterPurge.Content.ReadAsStringAsync();

        // After purge, the response should be empty or contain empty array
        Assert.IsTrue(
            string.IsNullOrWhiteSpace(afterContent) ||
            afterContent == "[]" ||
            !afterContent.Contains("EventBeforePurge"),
            "Telemetry should be cleared after purge"
        );
    }
}
