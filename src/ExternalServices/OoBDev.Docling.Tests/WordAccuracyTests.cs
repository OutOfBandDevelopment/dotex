using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Docling.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class WordAccuracyTests
{
    [TestMethod]
    public void Score_IdenticalIgnoringCaseAndPunctuation_IsOne()
    {
        Assert.AreEqual(1.0, WordAccuracy.Score("Total due: 12 dollars", "# TOTAL due 12\n\ndollars"));
    }

    [TestMethod]
    public void Score_OneWrongWordOfFour_IsThreeQuarters()
    {
        Assert.AreEqual(0.75, WordAccuracy.Score("one two three four", "one two thre four"), 1e-9);
    }

    [TestMethod]
    public void Score_NothingRecognised_IsZero()
    {
        Assert.AreEqual(0.0, WordAccuracy.Score("one two", string.Empty));
    }

    [TestMethod]
    public void RenderJpeg_SameSeed_GivesTheSamePdfBytes()
    {
        var first = SyntheticScan.Pdf(SyntheticScan.RenderJpeg());
        var second = SyntheticScan.Pdf(SyntheticScan.RenderJpeg());

        CollectionAssert.AreEqual(first, second);
        Assert.IsTrue(first.Length > 5000);
    }
}
