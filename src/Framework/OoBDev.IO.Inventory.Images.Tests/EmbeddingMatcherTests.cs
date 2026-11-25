using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.IO.Inventory.Images.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class EmbeddingMatcherTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Group_SimilarVectors_AreGroupedAndUnrelatedLeftOut()
    {
        // Stage
        var items = new (string, float[])[] { ("b.jpg", [1f, 0.01f, 0f]), ("a.jpg", [1f, 0f, 0f]), ("c.jpg", [0f, 1f, 0f]) };

        // Test
        var groups = new EmbeddingMatcher().Group(items, 0.97f);
        TestContext.WriteLine($"{groups.Count} group(s)");

        // Assert
        Assert.HasCount(1, groups);
        CollectionAssert.AreEqual(new[] { "a.jpg", "b.jpg" }, groups[0].ToArray());
    }

    [TestMethod]
    public void Group_MismatchedLengths_Throws()
    {
        var items = new (string, float[])[] { ("a", [1f, 0f]), ("b", [1f, 0f, 0f]) };

        Assert.Throws<ArgumentException>(() => new EmbeddingMatcher().Group(items, 0.9f));
    }
}
