using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Text.Json.Nodes;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class CloudEventMapperTests
{
    private static readonly DateTimeOffset Time = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToCloudEvent_WritesRequiredAttributes()
    {
        // Stage
        var record = new EventRecord("order-1", 3, "OrderPlaced", Time, "{\"total\":5}");

        // Test
        var json = JsonNode.Parse(CloudEventMapper.ToCloudEvent(record, "shop"))!;

        // Assert
        Assert.AreEqual("1.0", (string?)json["specversion"]);
        Assert.AreEqual("order-1:3", (string?)json["id"]);
        Assert.AreEqual("shop", (string?)json["source"]);
        Assert.AreEqual("OrderPlaced", (string?)json["type"]);
        Assert.AreEqual("order-1", (string?)json["subject"]);
        Assert.AreEqual(5, (int?)json["data"]!["total"]);
        Assert.IsNull(json["metadata"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void RoundTrip_ReturnsTheSameEvent()
    {
        // Stage
        var record = new EventRecord("order-1", 3, "OrderPlaced", Time, "{\"total\":5}", "{\"user\":\"a\"}");

        // Test
        var back = CloudEventMapper.FromCloudEvent(CloudEventMapper.ToCloudEvent(record, "shop"));

        // Assert
        Assert.AreEqual(record, back);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FromCloudEvent_ExternalEventWithoutExtensions_UsesTheIdAsStreamAndSequence()
    {
        // Stage
        const string json = """{"specversion":"1.0","id":"acct-9:12","source":"x","type":"Closed","time":"2026-11-11T12:00:00Z","data":{"a":1}}""";

        // Test
        var record = CloudEventMapper.FromCloudEvent(json);

        // Assert
        Assert.AreEqual("acct-9", record.StreamId);
        Assert.AreEqual(12, record.Sequence);
        Assert.AreEqual("{\"a\":1}", record.Payload);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("""{"specversion":"0.3","id":"a:1","type":"t","time":"2026-11-11T12:00:00Z"}""")]
    [DataRow("""{"specversion":"1.0","type":"t","time":"2026-11-11T12:00:00Z"}""")]
    [DataRow("""{"specversion":"1.0","id":"nocolon","type":"t","time":"2026-11-11T12:00:00Z"}""")]
    [DataRow("""{"specversion":"1.0","id":"a:1","type":"t"}""")]
    [DataRow("[]")]
    public void FromCloudEvent_InvalidDocument_ThrowsFormatException(string json)
    {
        // Test / Assert
        Assert.ThrowsExactly<FormatException>(() => CloudEventMapper.FromCloudEvent(json));
    }
}
