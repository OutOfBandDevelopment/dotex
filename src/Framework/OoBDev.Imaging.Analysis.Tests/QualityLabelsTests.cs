using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Imaging.Analysis.Bursts;
using OoBDev.TestUtilities;

namespace OoBDev.Imaging.Analysis.Tests;

[TestClass]
public class QualityLabelsTests
{
    private static readonly QualityMeasures Ok = new(1000, 0.8, 0.02, 120, 1, 1, 200, 2);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Of_GoodFrame_IsOk() => CollectionAssert.AreEqual(new[] { "ok" }, QualityLabels.Of(Ok).ToArray());

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(499.0, 0.1, 0.02, "shaky")]
    [DataRow(499.0, 0.5, 0.001, "defocused")]
    [DataRow(499.0, 0.5, 0.02, "soft")]
    [DataRow(500.0, 0.1, 0.001, "ok")]
    public void Of_Focus_Labels(double focus, double iso, double hf, string expected)
    {
        var labels = QualityLabels.Of(Ok with { FocusTile = focus, Isotropy = iso, HfShare = hf });

        CollectionAssert.AreEqual(new[] { expected }, labels.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(44.0, 1.0, 100.0, "under")]
    [DataRow(45.0, 36.0, 100.0, "under")]
    [DataRow(44.0, 1.0, 121.0, "dark")]
    [DataRow(45.0, 35.0, 100.0, "ok")]
    public void Of_Shadows_Labels(double mean, double clip, double range, string expected)
    {
        var labels = QualityLabels.Of(Ok with { LumaMean = mean, ClipShadowPct = clip, DynRange = range });

        CollectionAssert.AreEqual(new[] { expected }, labels.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(206.0, 1.0, "over")]
    [DataRow(205.0, 11.0, "over")]
    [DataRow(205.0, 10.0, "ok")]
    public void Of_Highlights_Labels(double mean, double clip, string expected)
    {
        var labels = QualityLabels.Of(Ok with { LumaMean = mean, ClipHighlightPct = clip });

        CollectionAssert.AreEqual(new[] { expected }, labels.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Of_NoiseAndLowRange_AreLabelled()
    {
        CollectionAssert.AreEqual(new[] { "noisy" }, QualityLabels.Of(Ok with { Noise = 6.1 }).ToArray());
        CollectionAssert.AreEqual(new[] { "flat" }, QualityLabels.Of(Ok with { DynRange = 59 }).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Of_LowRangeWithBadExposure_IsNotAlsoFlat()
    {
        var labels = QualityLabels.Of(Ok with { DynRange = 30, LumaMean = 20 });

        CollectionAssert.AreEqual(new[] { "under" }, labels.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Of_DarkFrame_IsNotUnder()
    {
        var labels = QualityLabels.Of(Ok with { DynRange = 130, LumaMean = 20 });

        CollectionAssert.AreEqual(new[] { "dark" }, labels.ToArray());
    }
}
