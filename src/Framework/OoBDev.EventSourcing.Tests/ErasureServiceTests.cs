using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class ErasureServiceTests
{
    [EventType("CustomerRegistered")]
    [PersonalData("CustomerId", "Name", "Email", "Age", "Nickname")]
    private sealed record CustomerRegistered(string CustomerId, string Name, string Email, int Age, string? Nickname, string Plan);

    [EventType("PlanChanged")]
    private sealed record PlanChanged(string CustomerId, string Plan);

    private sealed class Recorder : ISubjectErasureHandler
    {
        public List<string> Erased { get; } = [];

        public Task SubjectErasedAsync(string subjectId, CancellationToken cancellationToken = default)
        {
            Erased.Add(subjectId);
            return Task.CompletedTask;
        }
    }

    private readonly InMemoryEventStore _store = new(TimeProvider.System);
    private readonly Recorder _recorder = new();
    private ErasureService _service = null!;

    [TestInitialize]
    public void Initialize()
    {
        var registry = new EventTypeRegistry().Register<CustomerRegistered>().Register<PlanChanged>();
        _service = new ErasureService(_store, _store, registry, [_recorder]);
    }

    private Task<AppendResult> Append(string stream, string type, object body) => _store.AppendAsync(stream, 0, [new NewEvent(type, JsonSerializer.Serialize(body))]);

    private async Task Seed()
    {
        await Append("c1", "CustomerRegistered", new CustomerRegistered("c1", "Ann Lee", "ann@example.com", 41, "annie", "free"));
        await Append("c2", "CustomerRegistered", new CustomerRegistered("c2", "Bob Roe", "bob@example.com", 35, null, "pro"));
        await _store.AppendAsync("c1", 1, [new NewEvent("PlanChanged", JsonSerializer.Serialize(new PlanChanged("c1", "pro")))]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Compact_MarkedFields_AreBlankedAndOthersKept()
    {
        await Seed();
        await _service.RequestAsync("c1", "r1", ["backup-2026-11"]);

        var completed = await _service.CompactAsync();

        Assert.AreEqual(1, completed);
        var json = JsonDocument.Parse((await _store.ReadAsync("c1"))[0].Payload).RootElement;
        Assert.AreEqual("[erased]", json.GetProperty("Name").GetString());
        Assert.AreEqual("[erased]", json.GetProperty("Email").GetString());
        Assert.AreEqual(0, json.GetProperty("Age").GetInt32());
        Assert.AreEqual(JsonValueKind.Null, json.GetProperty("Nickname").ValueKind);
        Assert.AreEqual("c1", json.GetProperty("CustomerId").GetString());
        Assert.AreEqual("free", json.GetProperty("Plan").GetString());
        StringAssert.Contains((await _store.ReadAsync("c1"))[1].Payload, "pro");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Compact_OtherSubject_IsUntouched()
    {
        await Seed();
        await _service.RequestAsync("c1", "r1");

        await _service.CompactAsync();

        StringAssert.Contains((await _store.ReadAsync("c2"))[0].Payload, "bob@example.com");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Compact_SecondRun_ChangesNothing()
    {
        await Seed();
        await _service.RequestAsync("c1", "r1");
        await _service.CompactAsync();
        var before = await _store.ReadAllAsync(0);

        var again = await _service.CompactAsync();

        Assert.AreEqual(0, again);
        CollectionAssert.AreEqual(before.Select(e => e.Payload).ToList(), (await _store.ReadAllAsync(0)).Select(e => e.Payload).ToList());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Compact_RecordsCompletionWithCountAndNotifiesHandlers()
    {
        await Seed();
        await _service.RequestAsync("c1", "r1");

        await _service.CompactAsync();

        var stream = await _store.ReadAsync(ErasureService.StreamOf("c1"));
        Assert.AreEqual("ErasureRequested", stream[0].Type);
        Assert.AreEqual("ErasureCompleted", stream[1].Type);
        Assert.AreEqual(1, JsonDocument.Parse(stream[1].Payload).RootElement.GetProperty("EventsChanged").GetInt32());
        CollectionAssert.AreEqual(new[] { "c1" }, _recorder.Erased);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Compact_KeepsIdentityOfEvents()
    {
        await Seed();
        var before = (await _store.ReadAllAsync(0))[0];
        await _service.RequestAsync("c1", "r1");

        await _service.CompactAsync();

        var after = (await _store.ReadAllAsync(0))[0];
        Assert.AreEqual(before.Position, after.Position);
        Assert.AreEqual(before.Sequence, after.Sequence);
        Assert.AreEqual(before.Timestamp, after.Timestamp);
        Assert.AreEqual(before.Type, after.Type);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Compact_NoOpenRequest_ReturnsZero()
    {
        await Seed();

        Assert.AreEqual(0, await _service.CompactAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RewritePayloads_UnknownStream_ChangesNothing()
        => Assert.AreEqual(0, await _store.RewritePayloadsAsync("nope", new Dictionary<long, string> { [1] = "{}" }));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Register_MarkedPropertyMissing_Throws()
        => Assert.ThrowsExactly<InvalidOperationException>(() => new EventTypeRegistry().Register<BadMark>());

    [EventType("BadMark")]
    [PersonalData("Id", "Missing")]
    private sealed record BadMark(string Id);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void RuleFor_UnmarkedType_IsNull()
        => Assert.IsNull(new EventTypeRegistry().Register<PlanChanged>().RuleFor("PlanChanged"));

    [EventType("Unmarked")]
    private sealed record UnmarkedEvent(string SubjectId, string Email);

    [EventType("Reference")]
    [NoPersonalData]
    private sealed record ReferenceEvent(string SubjectId, string Plan);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Register_SubjectIdWithoutAMark_IsRefused()
    {
        var error = Assert.ThrowsExactly<InvalidOperationException>(() => new EventTypeRegistry().Register<UnmarkedEvent>());

        StringAssert.Contains(error.Message, "[PersonalData]");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Register_SubjectIdMarkedNoPersonalData_IsAcceptedWithoutARule()
        => Assert.IsNull(new EventTypeRegistry().Register<ReferenceEvent>().RuleFor("Reference"));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Register_ErasureEvents_AreAccepted()
        => _ = new EventTypeRegistry().Register<ErasureRequested>().Register<ErasureCompleted>();

    private sealed class Collecting : IProjection
    {
        public string Name => "collecting";

        public List<string> Payloads { get; } = [];

        public Task ApplyAsync(EventRecord record, CancellationToken cancellationToken = default)
        {
            Payloads.Add(record.Payload);
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Rebuild_AfterErasure_ReadModelHoldsNoPersonalValue()
    {
        // Stage
        await Seed();
        var projection = new Collecting();
        var runner = new ProjectionRunner(_store, new InMemoryEntityStore(), [projection]);
        _ = await runner.CatchUpAsync();
        Assert.IsTrue(projection.Payloads.Any(p => p.Contains("ann@example.com", StringComparison.Ordinal)));
        await _service.RequestAsync("c1", "r1");

        // Test
        _ = await _service.CompactAsync();
        projection.Payloads.Clear();
        await runner.ResetAsync(projection.Name);
        _ = await runner.CatchUpAsync();

        // Assert
        Assert.IsFalse(projection.Payloads.Any(p => p.Contains("ann@example.com", StringComparison.Ordinal) || p.Contains("Ann Lee", StringComparison.Ordinal)));
        Assert.IsTrue(projection.Payloads.Any(p => p.Contains("bob@example.com", StringComparison.Ordinal)));
    }
}
