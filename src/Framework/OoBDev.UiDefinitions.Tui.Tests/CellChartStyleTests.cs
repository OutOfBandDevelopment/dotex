using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.UiDefinitions.Tui.Tests;

[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class CellChartStyleTests
{
    [TestCleanup]
    public void Cleanup() => CellChartStyle.Source = null;

    [TestMethod]
    public void Muted_WithoutASource_IsNeutralGreyOnTheLightPalette()
    {
        // Stage
        CellChartStyle.Source = null;

        // Assert
        Assert.AreEqual("#808080", CellChartStyle.Muted);
        Assert.IsFalse(CellChartStyle.DarkPalette);
    }

    [TestMethod]
    public void Muted_WithASource_ReadsItOnEveryCall()
    {
        // Stage
        var muted = "#111111";
        CellChartStyle.Source = () => (muted, true);

        // Test
        muted = "#222222";

        // Assert
        Assert.AreEqual("#222222", CellChartStyle.Muted);
        Assert.IsTrue(CellChartStyle.DarkPalette);
    }
}
