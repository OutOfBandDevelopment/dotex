using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Linq;
using System.Text.Json.Nodes;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class EventSchemaRegistryTests
{
    [EventType("order-placed", Version = 2)]
    private sealed record OrderPlacedV2(string OrderId, int Quantity, string? Note = null);

    [EventType("order-placed")]
    private sealed record OrderPlacedV1(string OrderId, int Quantity);

    [EventType("order-shipped")]
    private sealed record OrderShipped(string OrderId);

    private static EventSchemaRegistry Registry<T>() => new(new EventTypeRegistry().Register<T>());

    private static JsonNode Schema<T>() => Registry<T>().Describe().Single().Schema;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_RegisteredTypes_ReturnsNameVersionAndSchemaOrderedByName()
    {
        // Stage
        var registry = new EventSchemaRegistry(new EventTypeRegistry().Register<OrderShipped>().Register<OrderPlacedV2>());

        // Test
        var schemas = registry.Describe();

        // Assert
        CollectionAssert.AreEqual(new[] { "order-placed", "order-shipped" }, schemas.Select(s => s.Name).ToArray());
        Assert.AreEqual(2, schemas[0].Version);
        Assert.AreEqual(1, schemas[1].Version);
        Assert.IsNotNull(schemas[0].Schema["properties"]?["orderId"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Validate_PayloadThatFits_ReturnsNoProblems()
    {
        var problems = Registry<OrderPlacedV2>().Validate("order-placed", """{"orderId":"A1","quantity":2,"note":null}""");

        Assert.AreEqual(0, problems.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Validate_MissingUnknownOrNullPayload_ReturnsProblems()
    {
        var registry = Registry<OrderPlacedV2>();

        Assert.AreEqual(1, registry.Validate("order-placed", """{"quantity":2}""").Count);
        Assert.AreEqual(1, registry.Validate("order-placed", """{"orderId":"A","quantity":2,"extra":1}""").Count);
        Assert.AreEqual(1, registry.Validate("order-placed", "null").Count);
        Assert.AreEqual(1, registry.Validate("order-placed", "not json").Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Compare_ClassifiesEachChange()
    {
        // Stage
        var v1 = Schema<OrderPlacedV1>();
        var v2 = Schema<OrderPlacedV2>();

        // Test
        var added = EventSchemaComparer.Compare(v1, v2);
        var removed = EventSchemaComparer.Compare(v2, v1);
        var same = EventSchemaComparer.Compare(v1, v1);

        // Assert
        Assert.AreEqual(ChangeKind.AddedOptional, added.Single().Kind);
        Assert.IsFalse(added.Single().IsBreaking);
        Assert.AreEqual(ChangeKind.Removed, removed.Single().Kind);
        Assert.IsTrue(removed.Single().IsBreaking);
        Assert.AreEqual(0, same.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Compare_RequiredAndTypeChanges_AreBreaking()
    {
        var old = JsonNode.Parse("""{"properties":{"a":{"type":"string"},"b":{"type":"integer"}},"required":["a"]}""")!;
        var current = JsonNode.Parse("""{"properties":{"a":{"type":"integer"},"b":{"type":"integer"},"c":{"type":"string"}},"required":["a","b","c"]}""")!;

        var changes = EventSchemaComparer.Compare(old, current);

        CollectionAssert.AreEqual(
            new[] { ("a", ChangeKind.TypeChanged), ("b", ChangeKind.BecameRequired), ("c", ChangeKind.AddedRequired) },
            changes.Select(c => (c.Property, c.Kind)).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Verify_UnchangedAndCompatible_ReturnsNoProblems()
    {
        var baseline = Registry<OrderPlacedV1>().ToSnapshot().ToJsonString();

        Assert.AreEqual(0, EventSchemaBaseline.Verify(Registry<OrderPlacedV1>(), baseline).Count);
        Assert.AreEqual(0, EventSchemaBaseline.Verify(Registry<OrderPlacedV2>(), baseline).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Verify_BreakingChangeWithoutVersionBump_ReportsIt()
    {
        var baseline = Registry<OrderPlacedV2>().ToSnapshot().ToJsonString();

        var problems = EventSchemaBaseline.Verify(Registry<OrderPlacedV1>(), baseline);

        Assert.IsTrue(problems.Any(p => p.Contains("version lowered", global::System.StringComparison.Ordinal)));
        Assert.IsTrue(problems.Any(p => p.Contains("note Removed", global::System.StringComparison.Ordinal)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Verify_BreakingChangeWithVersionBump_Passes()
    {
        var old = JsonNode.Parse(Registry<OrderPlacedV2>().ToSnapshot().ToJsonString())!;
        old["order-placed"]!["version"] = 1;
        var baseline = old.ToJsonString();

        Assert.AreEqual(0, EventSchemaBaseline.Verify(Registry<OrderPlacedV2>(), baseline).Count);

        var removedField = EventSchemaBaseline.Verify(Registry<OrderPlacedV1>(), baseline);
        Assert.AreEqual(1, removedField.Count);
        Assert.IsTrue(removedField[0].Contains("breaking change without a version bump", global::System.StringComparison.Ordinal));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Verify_TypeMissingFromRegistry_ReportsIt()
    {
        var baseline = Registry<OrderPlacedV1>().ToSnapshot().ToJsonString();

        var problems = EventSchemaBaseline.Verify(Registry<OrderShipped>(), baseline);

        Assert.AreEqual(1, problems.Count);
        Assert.IsTrue(problems[0].Contains("no longer registered", global::System.StringComparison.Ordinal));
    }
}
