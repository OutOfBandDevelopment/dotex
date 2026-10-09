using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Microsoft.ApplicationInsights.Extensibility;
using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using OoBDev.System.Security.Claims;
using OoBDev.TestUtilities;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.Microsoft.ApplicationInsights.Tests;

/// <summary>
/// Tests that the custom OpenTelemetry processors add correlation and user information to spans and log records.
/// </summary>
[TestClass]
public class TelemetryProcessorTests
{
    private const string SourceName = "OoBDev.Tests.TelemetryProcessors";

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CorrelationInfoTelemetryProcessor_ShouldAddCorrelationTags()
    {
        // Stage
        var info = new CorrelationInfo { CorrelationId = "corr-1", RequestId = "req-1" };
        var captured = new List<Activity>();

        // Test
        using (var provider = Sdk.CreateTracerProviderBuilder()
            .AddSource(SourceName)
            .AddProcessor(new CorrelationInfoTelemetryProcessor(new TestCorrelationAccessor(info)))
            .AddProcessor(new CaptureActivityProcessor(captured))
            .Build())
        using (var source = new ActivitySource(SourceName))
        using (source.StartActivity("work"))
        {
        }

        // Assert
        var activity = Assert.ContainsSingle(captured);
        Assert.AreEqual("corr-1", activity.GetTagItem(DefinedHttpHeaders.CorrelationIdHeader));
        Assert.AreEqual("req-1", activity.GetTagItem(DefinedHttpHeaders.RequestIdHeader));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void UserTelemetryProcessor_ShouldAddUserClaimTags()
    {
        // Stage
        var captured = new List<Activity>();

        // Test
        using (var provider = Sdk.CreateTracerProviderBuilder()
            .AddSource(SourceName)
            .AddProcessor(new UserTelemetryProcessor(CreateUserAccessor("obj-1", "user-1")))
            .AddProcessor(new CaptureActivityProcessor(captured))
            .Build())
        using (var source = new ActivitySource(SourceName))
        using (source.StartActivity("work"))
        {
        }

        // Assert
        var activity = Assert.ContainsSingle(captured);
        Assert.AreEqual("obj-1", activity.GetTagItem($"Claim-{CommonClaims.ObjectId}"));
        Assert.AreEqual("user-1", activity.GetTagItem($"Claim-{CommonClaims.UserId}"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void UserTelemetryProcessor_WithoutHttpContext_ShouldAddNothing()
    {
        // Stage
        var captured = new List<Activity>();

        // Test
        using (var provider = Sdk.CreateTracerProviderBuilder()
            .AddSource(SourceName)
            .AddProcessor(new UserTelemetryProcessor(new HttpContextAccessor()))
            .AddProcessor(new CaptureActivityProcessor(captured))
            .Build())
        using (var source = new ActivitySource(SourceName))
        using (source.StartActivity("work"))
        {
        }

        // Assert
        var activity = Assert.ContainsSingle(captured);
        Assert.IsFalse(activity.TagObjects.Any(t => t.Key.StartsWith("Claim-", StringComparison.Ordinal)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void LogProcessors_ShouldAddCorrelationAndUserAttributes()
    {
        // Stage
        var info = new CorrelationInfo { CorrelationId = "corr-2", RequestId = "req-2" };
        var captured = new List<Dictionary<string, object?>>();

        // Test
        using (var factory = LoggerFactory.Create(builder => builder.AddOpenTelemetry(options =>
        {
            options.AddProcessor(new CorrelationInfoLogProcessor(new TestCorrelationAccessor(info)));
            options.AddProcessor(new UserLogProcessor(CreateUserAccessor("obj-2", "user-2")));
            options.AddProcessor(new CaptureLogProcessor(captured));
        })))
        {
            factory.CreateLogger("test").LogInformation("hello {Name}", "world");
        }

        // Assert
        var attributes = Assert.ContainsSingle(captured);
        Assert.AreEqual("corr-2", attributes[DefinedHttpHeaders.CorrelationIdHeader]);
        Assert.AreEqual("req-2", attributes[DefinedHttpHeaders.RequestIdHeader]);
        Assert.AreEqual("obj-2", attributes[$"Claim-{CommonClaims.ObjectId}"]);
        Assert.AreEqual("user-2", attributes[$"Claim-{CommonClaims.UserId}"]);
        Assert.AreEqual("world", attributes["Name"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddApplicationInsightsExtensions_ShouldRegisterProcessors()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton<IAccessor<CorrelationInfo>>(new TestCorrelationAccessor(new CorrelationInfo()));
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());

        // Test
        services.TryAddApplicationInsightsExtensions();
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(provider.GetRequiredService<CorrelationInfoTelemetryProcessor>());
        Assert.IsNotNull(provider.GetRequiredService<UserTelemetryProcessor>());
        Assert.IsNotNull(provider.GetRequiredService<CorrelationInfoLogProcessor>());
        Assert.IsNotNull(provider.GetRequiredService<UserLogProcessor>());
    }

    private static TestHttpContextAccessor CreateUserAccessor(string objectId, string userId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(CommonClaims.ObjectId, objectId), new Claim(CommonClaims.UserId, userId)],
            "TestAuth");
        return new TestHttpContextAccessor(new DefaultHttpContext { User = new ClaimsPrincipal(identity) });
    }

    private sealed class CaptureActivityProcessor(List<Activity> captured) : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity data) => captured.Add(data);
    }

    private sealed class CaptureLogProcessor(List<Dictionary<string, object?>> captured) : BaseProcessor<LogRecord>
    {
        public override void OnEnd(LogRecord data)
            => captured.Add((data.Attributes ?? []).ToDictionary(a => a.Key, a => a.Value));
    }

    private sealed class TestCorrelationAccessor(CorrelationInfo value) : IAccessor<CorrelationInfo>
    {
        public CorrelationInfo? Value { get; set; } = value;
    }

    private sealed class TestHttpContextAccessor(HttpContext httpContext) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = httpContext;
    }
}
