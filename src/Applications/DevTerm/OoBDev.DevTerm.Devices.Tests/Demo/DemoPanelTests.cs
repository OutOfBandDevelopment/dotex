using OoBDev.DevTerm.Devices.Demo;

namespace OoBDev.DevTerm.Devices.Tests.Demo;

/// <summary>The demo panel: availability, definition and the line the Apply button sends.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class DemoPanelTests
{
    [TestMethod]
    public void IsAvailable_LoopbackAndSerial_OnlyLoopback()
    {
        // Stage
        var contribution = new DemoPanelContribution();

        // Test / Assert
        Assert.IsTrue(contribution.IsAvailable("loopback", 0, 0));
        Assert.IsFalse(contribution.IsAvailable("serial", 0, 0));
    }

    [TestMethod]
    public void BuildDefinition_Built_HasLedLevelAndApply()
    {
        // Test
        var definition = new DemoPanelContribution().BuildDefinition();

        // Assert
        var ids = definition.Sections.SelectMany(s => s.Controls).Select(c => c.Id).ToArray();
        CollectionAssert.AreEqual(new[] { "led", "level", "apply" }, ids);
    }

    [TestMethod]
    public async Task Invoke_LedLevelThenApply_SendsTheSetLine()
    {
        // Stage
        var transport = new FakeTransport();
        var session = new Session(transport, new Pipeline([]));
        await session.OpenAsync();
        var surface = new DemoPanelContribution().CreateSurface(session);

        // Test
        await surface.InvokeAsync("led", "true");
        await surface.InvokeAsync("level", "7");
        await surface.InvokeAsync("apply", null);

        // Assert
        CollectionAssert.AreEqual(new[] { "SET LED=1 LEVEL=7\r" }, transport.WrittenText.ToArray());
    }

    [TestMethod]
    public async Task Invoke_LevelOutOfRange_IsClamped()
    {
        // Stage
        var transport = new FakeTransport();
        var session = new Session(transport, new Pipeline([]));
        await session.OpenAsync();
        var surface = new DemoPanelContribution().CreateSurface(session);

        // Test
        await surface.InvokeAsync("level", "99");
        await surface.InvokeAsync("apply", null);

        // Assert
        CollectionAssert.AreEqual(new[] { "SET LED=0 LEVEL=9\r" }, transport.WrittenText.ToArray());
    }
}
