using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.Data.Vectors.Tests;

[TestClass]
public class SqlVectorFTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void MagnitudeTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        Assert.AreEqual(5.477225575051661, vector.Magnitude().Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ElementTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        Assert.AreEqual(1, vector.Element(0));
        Assert.AreEqual(2, vector.Element(1));
        Assert.AreEqual(3, vector.Element(2));
        Assert.AreEqual(4, vector.Element(3));
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow(VectorDistanceTypes.CosineDistance, 0.00619201f)]
    [DataRow(VectorDistanceTypes.CosineSimilarity, 0.993808f)]
    [DataRow(VectorDistanceTypes.DotProduct, 40.0f)]
    [DataRow(VectorDistanceTypes.EuclideanDistance, 2.0f)]
    [DataRow(VectorDistanceTypes.ManhattanDistance, 4.0f)]
    public void DistanceTest(string metric, float expected)
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        var vector2 = new SqlVectorF([2, 3, 4, 5]);
        Assert.AreEqual(expected, vector.Distance(vector2, metric));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AngleTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        var vector2 = new SqlVectorF([2, 3, 4, 5]);
        Assert.AreEqual(1.4130075f, vector.Angle(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CosineTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        var vector2 = new SqlVectorF([2, 3, 4, 5]);
        Assert.AreEqual(0.00619201f, vector.Cosine(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SimilarityTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        var vector2 = new SqlVectorF([2, 3, 4, 5]);
        Assert.AreEqual(0.993808f, vector.Similarity(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void DotProductTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        var vector2 = new SqlVectorF([2, 3, 4, 5]);
        Assert.AreEqual(40f, vector.DotProduct(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EuclideanTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        var vector2 = new SqlVectorF([2, 3, 4, 5]);
        Assert.AreEqual(2f, vector.Euclidean(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ManhattanTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        var vector2 = new SqlVectorF([2, 3, 4, 5]);
        Assert.AreEqual(4f, vector.Manhattan(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void MidpointTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        var vector2 = new SqlVectorF([2, 3, 4, 5]);
        Assert.AreEqual("[1.5000000e+000,2.5000000e+000,3.5000000e+000,4.5000000e+000]", vector.Midpoint(vector2).ToString());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void LengthTest()
    {
        var vector = new SqlVectorF([1, 2, 3, 4]);
        Assert.AreEqual(4, vector.Length());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ParseTest()
    {
        var vector = SqlVectorF.Parse("[1, 2, 3, 4]");
        Assert.AreEqual("[1.0000000e+000,2.0000000e+000,3.0000000e+000,4.0000000e+000]", vector.ToString());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToBytesTest()
    {
        var vector = SqlVectorF.Parse("[1, 2, 3, 4]");
        var bytes = Convert.ToBase64String(vector.ToBytes());
        Assert.AreEqual("BAAAAQAAgD8AAABAAABAQAAAgEBvRa9A", bytes);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FromTest()
    {
        var bytes = Convert.FromBase64String("BAAAAQAAgD8AAABAAABAQAAAgEBvRa9A");
        var vector = SqlVectorF.From(bytes);
        Assert.AreEqual("[1.0000000e+000,2.0000000e+000,3.0000000e+000,4.0000000e+000]", vector.ToString());
    }
}
