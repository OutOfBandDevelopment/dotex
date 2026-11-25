using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Examples.Controls.Tui.Tests;

/// <summary>The live displays fed by the simulated device; no terminal is started.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class LiveSampleTests
{
    [TestMethod]
    public void Constructor_CreatesAChartForTheBarGraphAndTheStripChartAndALabelForTheIndicator()
    {
        // Test
        var live = new LiveSample();

        // Assert
        Assert.HasCount(2, live.Charts);
        Assert.HasCount(1, live.IndicatorLabels);
    }

    [TestMethod]
    public void Step_PublishesASample_ToTheIndicatorAndTheCharts()
    {
        // Stage
        var live = new LiveSample();

        // Test
        live.Step();

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(live.IndicatorLabels.Single().Text));
        Assert.IsTrue(live.Charts[0].Render().Lines().Any(line => line.Contains('█')));
    }
}
