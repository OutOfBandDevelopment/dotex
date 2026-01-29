using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Microsoft.ApplicationInsights.Extensibility;
using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using OoBDev.System.Security.Claims;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.ApplicationInsights.Tests;

/// <summary>
/// Integration tests for custom Application Insights telemetry processors.
/// Tests verify that CorrelationInfoTelemetryProcessor and UserTelemetryProcessor
/// correctly add custom properties to telemetry items.
/// </summary>
[TestClass]
public class TelemetryProcessorTests
{
    private TelemetryClient? _telemetryClient;
    private TelemetryConfiguration? _configuration;
    private HttpClient? _httpClient;
    private ServiceProvider? _serviceProvider;

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
        await Task.Delay(1000);

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
        _serviceProvider?.Dispose();
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task CorrelationInfoTelemetryProcessor_ShouldAddCorrelationHeaders()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();

        var connectionString = TestContext!.GetRequiredProperty<string>("APPINSIGHTS_CONNECTION_STRING");
        var azurinsightUrl = TestContext!.GetRequiredProperty<string>("APPINSIGHTS_URL");

        // Setup DI container with correlation accessor
        var services = new ServiceCollection();

        var correlationInfo = new CorrelationInfo
        {
            CorrelationId = correlationId,
            RequestId = requestId
        };

        services.AddSingleton<IAccessor<CorrelationInfo>>(new TestCorrelationAccessor(correlationInfo));

        _configuration = TelemetryConfiguration.CreateDefault();
        _configuration.ConnectionString = connectionString;

        // Use InMemoryChannel for immediate transmission (important for testing)
        _configuration.TelemetryChannel = new InMemoryChannel
        {
            EndpointAddress = azurinsightUrl + "/v2.1/track"
        };

        // Add the correlation processor
        var processorFactory = new TestTelemetryProcessorFactory<CorrelationInfoTelemetryProcessor>(services.BuildServiceProvider());
        _configuration.TelemetryProcessorChainBuilder.Use(processorFactory.Create);
        _configuration.TelemetryProcessorChainBuilder.Build();

        _telemetryClient = new TelemetryClient(_configuration);

        // Act
        _telemetryClient.TrackEvent("CorrelationTest");
        _telemetryClient.Flush();
        await Task.Delay(2000);

        // Assert
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(correlationId, content, $"CorrelationId '{correlationId}' not found in telemetry");
        Assert.Contains(requestId, content, $"RequestId '{requestId}' not found in telemetry");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task UserTelemetryProcessor_ShouldAddUserClaims()
    {
        // Arrange
        var objectId = "test-object-id-123";
        var userId = "test-user-id-456";

        var connectionString = TestContext!.GetRequiredProperty<string>("APPINSIGHTS_CONNECTION_STRING");

        this.TestContext.WriteLine($"{nameof(connectionString)}: {connectionString}");

        // Setup DI container with HTTP context accessor
        var services = new ServiceCollection();

        var claims = new List<Claim>
        {
            new(CommonClaims.ObjectId, objectId),
            new(CommonClaims.UserId, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var httpContextAccessor = new TestHttpContextAccessor(httpContext);
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

        var azurinsightUrl = TestContext!.GetRequiredProperty<string>("APPINSIGHTS_URL");

        _configuration = TelemetryConfiguration.CreateDefault();
        _configuration.ConnectionString = connectionString;

        // Use InMemoryChannel for immediate transmission (important for testing)
        _configuration.TelemetryChannel = new InMemoryChannel
        {
            EndpointAddress = azurinsightUrl + "/v2.1/track"
        };

        // Add the user processor
        var processorFactory = new TestTelemetryProcessorFactory<UserTelemetryProcessor>(services.BuildServiceProvider());
        _configuration.TelemetryProcessorChainBuilder.Use(processorFactory.Create);
        _configuration.TelemetryProcessorChainBuilder.Build();

        _telemetryClient = new TelemetryClient(_configuration);

        // Act
        _telemetryClient.TrackEvent("UserTest");
        _telemetryClient.Flush();
        await Task.Delay(2000);

        // Assert
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(objectId, content, $"ObjectId '{objectId}' not found in telemetry");
        Assert.Contains(userId, content, $"UserId '{userId}' not found in telemetry");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task CombinedProcessors_ShouldAddBothCorrelationAndUserInfo()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();
        var objectId = "combined-object-id";
        var userId = "combined-user-id";

        var connectionString = TestContext!.GetRequiredProperty<string>("APPINSIGHTS_CONNECTION_STRING");

        // Setup DI container with both accessors
        var services = new ServiceCollection();

        // Add correlation accessor
        var correlationInfo = new CorrelationInfo
        {
            CorrelationId = correlationId,
            RequestId = requestId
        };
        services.AddSingleton<IAccessor<CorrelationInfo>>(new TestCorrelationAccessor(correlationInfo));

        // Add HTTP context accessor
        var claims = new List<Claim>
        {
            new(CommonClaims.ObjectId, objectId),
            new(CommonClaims.UserId, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        services.AddSingleton<IHttpContextAccessor>(new TestHttpContextAccessor(httpContext));

        _serviceProvider = services.BuildServiceProvider();

        var azurinsightUrl = TestContext!.GetRequiredProperty<string>("APPINSIGHTS_URL");

        _configuration = TelemetryConfiguration.CreateDefault();
        _configuration.ConnectionString = connectionString;

        // Use InMemoryChannel for immediate transmission (important for testing)
        _configuration.TelemetryChannel = new InMemoryChannel
        {
            EndpointAddress = azurinsightUrl + "/v2.1/track"
        };

        // Add both processors
        var correlationFactory = new TestTelemetryProcessorFactory<CorrelationInfoTelemetryProcessor>(_serviceProvider);
        var userFactory = new TestTelemetryProcessorFactory<UserTelemetryProcessor>(_serviceProvider);

        _configuration.TelemetryProcessorChainBuilder.Use(correlationFactory.Create);
        _configuration.TelemetryProcessorChainBuilder.Use(userFactory.Create);
        _configuration.TelemetryProcessorChainBuilder.Build();

        _telemetryClient = new TelemetryClient(_configuration);

        // Act
        _telemetryClient.TrackEvent("CombinedTest");
        _telemetryClient.Flush();
        await Task.Delay(2000);

        // Assert
        var response = await _httpClient!.GetAsync("/api/query");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        // Verify correlation info
        Assert.Contains(correlationId, content, $"CorrelationId '{correlationId}' not found");
        Assert.Contains(requestId, content, $"RequestId '{requestId}' not found");

        // Verify user info
        Assert.Contains(objectId, content, $"ObjectId '{objectId}' not found");
        Assert.Contains(userId, content, $"UserId '{userId}' not found");
    }

    #region Test Helpers

    private class TestCorrelationAccessor : IAccessor<CorrelationInfo>
    {
        public CorrelationInfo? Value { get; set; }

        public TestCorrelationAccessor(CorrelationInfo value) => Value = value;
    }

    private class TestHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }

        public TestHttpContextAccessor(HttpContext httpContext) => HttpContext = httpContext;
    }

    private class TestTelemetryProcessorFactory<T> where T : ITelemetryProcessor
    {
        private readonly IServiceProvider _serviceProvider;

        public TestTelemetryProcessorFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public ITelemetryProcessor Create(ITelemetryProcessor next)
        {
            return (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T), next);
        }
    }

    #endregion
}
