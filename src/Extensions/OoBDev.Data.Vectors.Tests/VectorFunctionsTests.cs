using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Linq;

namespace OoBDev.Data.Vectors.Tests;

[TestClass]
public class VectorFunctionsTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void LengthTest()
    {
        Assert.IsTrue(VectorFunctions.Length(SqlVector.Null).IsNull);
        Assert.AreEqual(3, VectorFunctions.Length(new SqlVector([1.0, 2.3, 4.5])).Value);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void RandomTest()
    {
        var vector = VectorFunctions.Random(100, 0);
        this.TestContext.WriteLine(vector.ToString());
        Assert.AreEqual(100, vector.Length());
        Assert.IsLessThan(1.0, vector.Values.Min());
        Assert.IsGreaterThan(-1.0, vector.Values.Max());
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void UniformTest()
    {
        var targetLength = 20;

        var vector = VectorFunctions.Uniform(targetLength, 10, 15, 0);
        this.TestContext.WriteLine(vector.ToString());
        Assert.AreEqual(targetLength, vector.Length());
        Assert.IsGreaterThanOrEqualTo(10.0, vector.Values.Min());
        Assert.IsLessThanOrEqualTo(15.0, vector.Values.Max());
    }
}
