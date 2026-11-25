using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Orchestration;
using OoBDev.TestUtilities;

namespace OoBDev.Communications.Tests;

[TestClass]
public class CommunicationOrchestratorTests
{
    public required TestContext TestContext { get; set; }

    private sealed class SmsComposer : IMessageComposer
    {
        public string ChannelName => "sms";

        public Task<IMessage?> ComposeAsync(CommunicationRequest request, TargetPreference preference, CancellationToken cancellationToken = default)
            => Task.FromResult<IMessage?>(new SmsMessage { To = "+15551230000", Body = $"{request.MessageType}:{request.Data["name"]}:{preference.Culture.Name}" });
    }

    private sealed class NameEnhancer : IDataEnhancer
    {
        public bool AppliesTo(string messageType) => messageType == "order.shipped";

        public Task EnhanceAsync(CommunicationRequest request, IDictionary<string, object?> data, CancellationToken cancellationToken = default)
        {
            data["name"] = "Ada";
            return Task.CompletedTask;
        }
    }

    private sealed record Fixture(CommunicationOrchestrator Orchestrator, Mock<ISendService> Sender, InMemoryTargetPreferenceProvider Preferences, InMemoryDeferralStore Store, FakeTimeProvider Time, List<IMessage> Sent);

    private static Fixture Build(DateTimeOffset now)
    {
        var sent = new List<IMessage>();
        var sender = new Mock<ISendService>();
        sender.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<IMessage>(), It.IsAny<SendOptions?>(), It.IsAny<CancellationToken>()))
            .Callback<string, IMessage, SendOptions?, CancellationToken>((_, m, _, _) => sent.Add(m))
            .ReturnsAsync((string _, IMessage m, SendOptions? o, CancellationToken _) => new SendResult(true, m.MessageId, o?.CorrelationId, "p", now));
        var preferences = new InMemoryTargetPreferenceProvider();
        var store = new InMemoryDeferralStore();
        var time = new FakeTimeProvider(now);
        var orchestrator = new CommunicationOrchestrator(preferences, [new NameEnhancer()], [new SmsComposer()], sender.Object, store, time);
        return new Fixture(orchestrator, sender, preferences, store, time, sent);
    }

    private static TargetPreference Pref(RequestPriority minimum = RequestPriority.Normal, TimeOnly? start = null, TimeOnly? end = null, params string[] channels)
        => new("ada", channels.Length == 0 ? ["sms"] : channels, CultureInfo.GetCultureInfo("fr-FR"), TimeZoneInfo.Utc, minimum, start, end);

    private static CommunicationRequest Request(RequestPriority priority = RequestPriority.Normal)
        => new("ada", "order.shipped", new Dictionary<string, object?>(), priority);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_PreferredChannel_EnhancesComposesAndSends()
    {
        // Stage
        var f = Build(new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero));
        f.Preferences.Set(Pref());

        // Test
        var result = await f.Orchestrator.SendAsync(Request(), TestContext.CancellationToken);
        TestContext.WriteLine($"{result.Outcome} {result.CorrelationId} {((ISmsMessage)f.Sent[0]).Body}");

        // Assert
        Assert.AreEqual(CommunicationOutcome.Sent, result.Outcome);
        Assert.HasCount(1, result.Results);
        Assert.AreEqual("order.shipped:Ada:fr-FR", ((ISmsMessage)f.Sent[0]).Body);
        Assert.AreEqual(result.CorrelationId, result.Results[0].CorrelationId);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_UnknownTarget_IsSkipped()
    {
        // Test
        var f = Build(DateTimeOffset.UnixEpoch);
        var result = await f.Orchestrator.SendAsync(Request(), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(CommunicationOutcome.Skipped, result.Outcome);
        Assert.IsEmpty(f.Sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_BelowMinimumPriority_IsSkippedButCriticalPasses()
    {
        // Stage
        var f = Build(new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero));
        f.Preferences.Set(Pref(minimum: RequestPriority.High));

        // Test
        var normal = await f.Orchestrator.SendAsync(Request(RequestPriority.Normal), TestContext.CancellationToken);
        var critical = await f.Orchestrator.SendAsync(Request(RequestPriority.Critical), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(CommunicationOutcome.Skipped, normal.Outcome);
        Assert.AreEqual(CommunicationOutcome.Sent, critical.Outcome);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_ChannelWithoutComposer_IsSkippedAsNothingComposed()
    {
        // Stage
        var f = Build(new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero));
        f.Preferences.Set(Pref(channels: "push"));

        // Test
        var result = await f.Orchestrator.SendAsync(Request(), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(CommunicationOutcome.Skipped, result.Outcome);
        Assert.Contains("No channel", result.Reason ?? string.Empty);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_NormalInQuietHours_DefersUntilTheWindowEndsThenReleases()
    {
        // Stage: 23:00 UTC, quiet 22:00 to 08:00
        var f = Build(new DateTimeOffset(2026, 11, 25, 23, 0, 0, TimeSpan.Zero));
        f.Preferences.Set(Pref(start: new TimeOnly(22, 0), end: new TimeOnly(8, 0)));

        // Test
        var result = await f.Orchestrator.SendAsync(Request(), TestContext.CancellationToken);
        var early = await f.Orchestrator.ReleaseDueAsync(TestContext.CancellationToken);
        f.Time.SetUtcNow(new DateTimeOffset(2026, 11, 26, 8, 0, 0, TimeSpan.Zero));
        var released = await f.Orchestrator.ReleaseDueAsync(TestContext.CancellationToken);
        TestContext.WriteLine($"{result.Reason}; early={early} released={released} sent={f.Sent.Count}");

        // Assert
        Assert.AreEqual(CommunicationOutcome.Deferred, result.Outcome);
        Assert.AreEqual(0, early);
        Assert.AreEqual(1, released);
        Assert.HasCount(1, f.Sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_HighInQuietHours_SendsAtOnce()
    {
        // Stage
        var f = Build(new DateTimeOffset(2026, 11, 25, 23, 0, 0, TimeSpan.Zero));
        f.Preferences.Set(Pref(start: new TimeOnly(22, 0), end: new TimeOnly(8, 0)));

        // Test
        var result = await f.Orchestrator.SendAsync(Request(RequestPriority.High), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(CommunicationOutcome.Sent, result.Outcome);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task DeferAsync_HoldsUntilDueAndReReadsPreferences()
    {
        // Stage
        var f = Build(new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero));
        f.Preferences.Set(Pref());

        // Test
        var deferred = await f.Orchestrator.DeferAsync(Request(), f.Time.GetUtcNow().AddHours(1), TestContext.CancellationToken);
        f.Preferences.Set(Pref(minimum: RequestPriority.Critical));
        f.Time.Advance(TimeSpan.FromHours(2));
        var released = await f.Orchestrator.ReleaseDueAsync(TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(CommunicationOutcome.Deferred, deferred.Outcome);
        Assert.AreEqual(1, released);
        Assert.IsEmpty(f.Sent, "the changed preference drops the request at release");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void QuietHours_SameDayWindowAndNoWindow_AreHandled()
    {
        // Test
        var day = Pref(start: new TimeOnly(9, 0), end: new TimeOnly(17, 0));
        var none = Pref();

        // Assert
        Assert.IsTrue(QuietHours.IsQuiet(day, new DateTimeOffset(2026, 11, 25, 10, 0, 0, TimeSpan.Zero)));
        Assert.IsFalse(QuietHours.IsQuiet(day, new DateTimeOffset(2026, 11, 25, 17, 0, 0, TimeSpan.Zero)));
        Assert.IsFalse(QuietHours.IsQuiet(none, DateTimeOffset.UnixEpoch));
        Assert.AreEqual(new DateTimeOffset(2026, 11, 25, 17, 0, 0, TimeSpan.Zero), QuietHours.NextEnd(day, new DateTimeOffset(2026, 11, 25, 10, 0, 0, TimeSpan.Zero)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void QuietHours_TargetTimeZone_IsHonored()
    {
        // Stage: 03:00 UTC is 22:00 the day before in UTC-5
        var zone = TimeZoneInfo.CreateCustomTimeZone("minus5", TimeSpan.FromHours(-5), "minus5", "minus5");
        var pref = Pref(start: new TimeOnly(22, 0), end: new TimeOnly(8, 0)) with { TimeZone = zone };

        // Assert
        Assert.IsTrue(QuietHours.IsQuiet(pref, new DateTimeOffset(2026, 11, 26, 3, 0, 0, TimeSpan.Zero)));
        Assert.AreEqual(new DateTimeOffset(2026, 11, 26, 13, 0, 0, TimeSpan.Zero), QuietHours.NextEnd(pref, new DateTimeOffset(2026, 11, 26, 3, 0, 0, TimeSpan.Zero)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddCommunicationOrchestration_RegistersTheOrchestratorOnce()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ISendService>());

        // Test
        services.TryAddCommunicationOrchestration().TryAddCommunicationOrchestration();
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<CommunicationOrchestrator>(provider.GetRequiredService<ICommunicationOrchestrator>());
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(ICommunicationOrchestrator)));
    }
}
