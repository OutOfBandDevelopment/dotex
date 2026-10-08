using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Data.Vectors.Tests;

/// <summary>
/// Undefined input returns SQL NULL instead of throwing, so one bad row never fails a whole SQL batch.
/// </summary>
[TestClass]
public class NullSafetyTests
{
    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("1,x,3")]
    [DataRow("[1,2,")]
    [DataRow("   ")]
    public void VectorParse_BadInput_ReturnsNull(string input)
    {
        Assert.IsTrue(SqlVector.Parse(input).IsNull);
        Assert.IsTrue(SqlVectorF.Parse(input).IsNull);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("")]
    [DataRow("1,2|3")]
    [DataRow("1,2|3,4,5")]
    [DataRow("1,a|3,4")]
    public void MatrixParse_BadInput_ReturnsNull(string input)
    {
        Assert.IsTrue(SqlMatrix.Parse(input).IsNull);
        Assert.IsTrue(SqlMatrixF.Parse(input).IsNull);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow((short)-1, (short)0)]
    [DataRow((short)2, (short)0)]
    [DataRow((short)0, (short)-1)]
    [DataRow((short)0, (short)3)]
    public void MatrixAccessors_OutOfRange_ReturnNull(short row, short column)
    {
        var matrix = SqlMatrix.Parse("1,2,3|4,5,6");
        var matrixF = SqlMatrixF.Parse("1,2,3|4,5,6");

        Assert.IsTrue(matrix.Element(row, column).IsNull);
        Assert.IsTrue(matrixF.Element(row, column).IsNull);
        Assert.IsTrue(matrix.Row(row).IsNull || row is >= 0 and < 2);
        Assert.IsTrue(matrixF.Column(column).IsNull || column is >= 0 and < 3);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void MatrixAccessors_InRange_ReturnValues()
    {
        var matrix = SqlMatrix.Parse("1.25,2|3,4");

        Assert.AreEqual(1.25, matrix.Element(0, 0).Value);
        Assert.AreEqual(4d, matrix.Element(1, 1).Value);
        Assert.IsFalse(matrix.Row(1).IsNull);
        Assert.IsFalse(matrix.Column(1).IsNull);
    }
}
