using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class EventUpcastPipelineTests
{
    private sealed class Step(string from, string to, string suffix) : IEventUpcaster
    {
        public string FromType { get; } = from;
        public (string Type, string Payload) Upcast(string payload) => (to, payload + suffix);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Upcast_NoMatch_ReturnsUnchanged()
    {
        // Test
        var result = new EventUpcastPipeline([]).Upcast("A", "p");

        // Assert
        Assert.AreEqual(("A", "p"), result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Upcast_Chain_AppliesEveryStep()
    {
        // Stage
        var pipeline = new EventUpcastPipeline([new Step("V1", "V2", "1"), new Step("V2", "V3", "2")]);

        // Test
        var result = pipeline.Upcast("V1", "p");

        // Assert
        Assert.AreEqual(("V3", "p12"), result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Upcast_Cycle_Throws()
    {
        // Stage
        var pipeline = new EventUpcastPipeline([new Step("A", "B", ""), new Step("B", "A", "")]);

        // Test and Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => pipeline.Upcast("A", "p"));
    }
}
