using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Math;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.Math;

[TestClass]
public class VectorComparerTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Compare_IdenticalVectors_ReturnsZero()
    {
        // Stage
        var comparer = new VectorComparer { DistanceMetric = VectorDistanceMetrics.Euclidean };

        // Test
        var result = comparer.Compare(new[] { 1d, 2d }, new[] { 1d, 2d });

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Compare_DifferentVectors_ReturnsPositive()
    {
        // Stage
        var comparer = new VectorComparer { DistanceMetric = VectorDistanceMetrics.Euclidean };

        // Test
        var result = comparer.Compare(new[] { 1d, 2d }, new[] { 4d, 6d });

        // Assert
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Compare_BothNull_ReturnsZero()
    {
        // Stage
        var comparer = new VectorComparer();

        // Test
        var result = comparer.Compare((object?)null, null);

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CosineDistance_ZeroMagnitude_ReturnsOne()
    {
        // Test
        var result = VectorMath.CosineDistance([0d, 0d], 0d, [1d, 1d], global::System.Math.Sqrt(2));

        // Assert
        Assert.AreEqual(1d, result);
    }
}
