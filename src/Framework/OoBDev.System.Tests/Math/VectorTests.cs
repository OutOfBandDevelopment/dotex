using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Math;

namespace OoBDev.System.Tests.Math;

[TestClass]
public class VectorTests
{
    public TestContext TestContext { get; set; } = null!;

    [DataTestMethod]
    [TestCategory("UNIT")]
    [DataRow("[1,2,3,4]", 4, 5.477225575051661d)]
    [DataRow("[1,1,1,1]", 4, 2)]
    public void Test(string vectorValue, int expectedLength, double expectedMagnitude)
    {
        var vector = Vector.Parse(vectorValue);
        Assert.AreEqual(expectedLength, vector.Value.Length);
        Assert.AreEqual(expectedMagnitude, vector.Magnitude);
    }

    [DataTestMethod]
    [TestCategory("UNIT")]
    [DataRow("[1,2,3,4]", "[1,2,3,4]", VectorDistanceMetrics.Cosine, 0)]
    [DataRow("[1,2,3,4]", "[1,2,3,4]", VectorDistanceMetrics.Euclidean, 0)]
    [DataRow("[1,2,3,4]", "[1,2,3,4]", VectorDistanceMetrics.DotProduct, 30)]
    [DataRow("[1,2,3,4]", "[-1,2,3,4]", VectorDistanceMetrics.Cosine, 0.06666666666666665d)]
    [DataRow("[-1,2,3,4]", "[1,2,3,4]", VectorDistanceMetrics.Cosine, 0.06666666666666665d)]
    [DataRow("[1,2,3,4]", "[1,-2,3,4]", VectorDistanceMetrics.Euclidean, 4)]
    [DataRow("[1,2,3,4]", "[1,2,-3,4]", VectorDistanceMetrics.DotProduct, 12)]
    [DataRow("[1,2,3,4]", "[1,2,3,-4]", VectorDistanceMetrics.Manhattan, 8)]
    public void Test(string vector1Value, string vector2Value, VectorDistanceMetrics metric, double expectedDistance)
    {
        var vector1 = Vector.Parse(vector1Value);
        var vector2 = Vector.Parse(vector2Value);
        Assert.AreEqual(expectedDistance, vector1.Distance(vector2, metric));
    }   
}
