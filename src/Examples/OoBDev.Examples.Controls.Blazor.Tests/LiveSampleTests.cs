using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Examples.Controls.Blazor.Components;

namespace OoBDev.Examples.Controls.Blazor.Tests;

/// <summary>The live sample draws an indicator, a bar graph and a strip chart, and each simulated step moves them.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class LiveSampleTests
{
    [TestMethod]
    public void Step_PublishesSamples_ToIndicatorBarsAndChart()
    {
        // Stage
        using var context = new Bunit.TestContext();
        var cut = context.RenderComponent<LiveSample>(p => p.Add(c => c.AutoRun, false));
        var before = cut.Find("[data-indicator=sum]").TextContent;

        // Test
        cut.Instance.Step();
        cut.Instance.Step();

        // Assert
        Assert.AreEqual("-", before);
        Assert.AreNotEqual("-", cut.Find("[data-indicator=sum]").TextContent);
        Assert.AreEqual(2, cut.FindAll("polyline").Count);
        Assert.AreEqual(2, cut.FindAll(".uidef-bar").Count);
    }
}
