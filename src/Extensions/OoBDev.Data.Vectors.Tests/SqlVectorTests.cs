using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.Data.Vectors.Tests;

[TestClass]
public class SqlVectorTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void MagnitudeTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        Assert.AreEqual(5.477225575051661, vector.Magnitude());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ElementTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        Assert.AreEqual(1, vector.Element(0));
        Assert.AreEqual(2, vector.Element(1));
        Assert.AreEqual(3, vector.Element(2));
        Assert.AreEqual(4, vector.Element(3));
    }

    [DataTestMethod]
    [DataRow(VectorDistanceTypes.CosineDistance, 0.006192010000093506)]
    [DataRow(VectorDistanceTypes.CosineSimilarity, 0.9938079899999065)]
    [DataRow(VectorDistanceTypes.DotProduct, 40.0)]
    [DataRow(VectorDistanceTypes.EuclideanDistance, 2)]
    [DataRow(VectorDistanceTypes.ManhattanDistance, 4)]
    [TestCategory(TestCategories.Unit)]
    public void DistanceTest(string metric, double expected)
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        var vector2 = new SqlVector([2, 3, 4, 5]);
        Assert.AreEqual(expected, vector.Distance(vector2, metric));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AngleTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        var vector2 = new SqlVector([2, 3, 4, 5]);
        Assert.AreEqual(1.4130075487425158, vector.Angle(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CosineTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        var vector2 = new SqlVector([2, 3, 4, 5]);
        Assert.AreEqual(0.006192010000093506, vector.Cosine(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SimilarityTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        var vector2 = new SqlVector([2, 3, 4, 5]);
        Assert.AreEqual(0.9938079899999065, vector.Similarity(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void DotProductTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        var vector2 = new SqlVector([2, 3, 4, 5]);
        Assert.AreEqual(40.0, vector.DotProduct(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EuclideanTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        var vector2 = new SqlVector([2, 3, 4, 5]);
        Assert.AreEqual(2.0, vector.Euclidean(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ManhattanTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        var vector2 = new SqlVector([2, 3, 4, 5]);
        Assert.AreEqual(4.0, vector.Manhattan(vector2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void MidpointTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        var vector2 = new SqlVector([2, 3, 4, 5]);
        Assert.AreEqual("[1.5000000e+000,2.5000000e+000,3.5000000e+000,4.5000000e+000]", vector.Midpoint(vector2).ToString());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void LengthTest()
    {
        var vector = new SqlVector([1, 2, 3, 4]);
        Assert.AreEqual(4, vector.Length());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ParseTest()
    {
        var vector = SqlVector.Parse("[1, 2, 3, 4]");
        Assert.AreEqual("[1.0000000e+000,2.0000000e+000,3.0000000e+000,4.0000000e+000]", vector.ToString());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToBytesTest()
    {
        var vector = SqlVector.Parse("[1, 2, 3, 4]");
        var bytes = Convert.ToBase64String(vector.ToBytes());
        Assert.AreEqual("BAAAAQAAAAAAAPA/AAAAAAAAAEAAAAAAAAAIQAAAAAAAABBAj6U20q3oFUA=", bytes);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FromTest()
    {
        var bytes = Convert.FromBase64String("BAAAAQAAAAAAAPA/AAAAAAAAAEAAAAAAAAAIQAAAAAAAABBAj6U20q3oFUA=");
        var vector = SqlVector.From(bytes);
        Assert.AreEqual("[1.0000000e+000,2.0000000e+000,3.0000000e+000,4.0000000e+000]", vector.ToString());
    }
}
