using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>The Survive reconnect rules of REQ-011 (token, new session, window).</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ReconnectRegistryTests
{
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Now;
    }

    [TestMethod]
    public void Reconnect_WithinWindow_GivesNewSessionAndKey()
    {
        var clock = new Clock();
        var registry = new ReconnectRegistry(clock, TimeSpan.FromSeconds(30));
        var token = registry.Detach("telemetry", lastSessionNumber: 1);
        clock.Now += TimeSpan.FromSeconds(10);

        var result = registry.Reconnect("telemetry", token);

        Assert.AreEqual(ReconnectRefusal.None, result.Refusal);
        Assert.AreEqual(2, result.SessionNumber);
        Assert.HasCount(32, result.SessionKey!);
    }

    [TestMethod]
    public void Reconnect_TokenUsedTwice_RefusesTheSecondUse()
    {
        var registry = new ReconnectRegistry(new Clock(), TimeSpan.FromSeconds(30));
        var token = registry.Detach("telemetry", 1);
        registry.Reconnect("telemetry", token);

        Assert.AreEqual(ReconnectRefusal.Unknown, registry.Reconnect("telemetry", token).Refusal);
    }

    [TestMethod]
    public void Reconnect_AfterWindow_IsRefusedAsExpired()
    {
        var clock = new Clock();
        var registry = new ReconnectRegistry(clock, TimeSpan.FromSeconds(30));
        var token = registry.Detach("telemetry", 1);
        clock.Now += TimeSpan.FromSeconds(31);

        Assert.AreEqual(ReconnectRefusal.Expired, registry.Reconnect("telemetry", token).Refusal);
    }

    [TestMethod]
    public void Reconnect_OtherPluginsToken_IsRefusedAndKeepsTheToken()
    {
        var registry = new ReconnectRegistry(new Clock(), TimeSpan.FromSeconds(30));
        var token = registry.Detach("telemetry", 1);

        Assert.AreEqual(ReconnectRefusal.WrongPlugin, registry.Reconnect("analytics", token).Refusal);
        Assert.AreEqual(ReconnectRefusal.None, registry.Reconnect("telemetry", token).Refusal);
    }

    [TestMethod]
    public void Reconnect_ForgedToken_IsRefused()
    {
        var registry = new ReconnectRegistry(new Clock(), TimeSpan.FromSeconds(30));
        registry.Detach("telemetry", 1);

        Assert.AreEqual(ReconnectRefusal.Unknown, registry.Reconnect("telemetry", "forged").Refusal);
    }

    [TestMethod]
    public void Sweep_AfterWindow_ReportsThePluginToKillOnce()
    {
        var clock = new Clock();
        var registry = new ReconnectRegistry(clock, TimeSpan.FromSeconds(30));
        registry.Detach("telemetry", 1);
        clock.Now += TimeSpan.FromSeconds(31);

        CollectionAssert.AreEqual(new[] { "telemetry" }, new global::System.Collections.Generic.List<string>(registry.Sweep()));
        Assert.IsEmpty(registry.Sweep());
    }
}
