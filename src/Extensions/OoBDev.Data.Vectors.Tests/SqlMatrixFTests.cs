using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.Data.Vectors.Tests;

[TestClass]
public class SqlMatrixFTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("1,2,3\r\n4,5,7", @"1.0000000e+000	2.0000000e+000	3.0000000e+000
4.0000000e+000	5.0000000e+000	7.0000000e+000")]
    [DataRow("1,2,3|4,5,7", "1,2,3|4,5,7")]
    [DataRow("1,2,3\n4,5,7", "1\t,2\t,3\r4\t5\t7")]
    public void ParseTest(string input, string expectedString)
    {
        var matrix = SqlMatrixF.Parse(input);
        var expected = SqlMatrixF.Parse(expectedString);
        Assert.AreEqual(expected, matrix);
    }

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("1,2,3|4,5,7", (short)1, "4,5,7")]
    [DataRow("1,2,3|4,5,7", (short)0, "1,2,3")]
    public void RowTest(string input, short rowIndex, string expectedString)
    {
        var matrix = SqlMatrixF.Parse(input);
        this.TestContext.WriteLine("matrix:");
        this.TestContext.WriteLine(matrix.ToString());

        var row = matrix.Row(rowIndex);
        this.TestContext.WriteLine("row:");
        this.TestContext.WriteLine(row.ToString());

        var expected = SqlVectorF.Parse(expectedString);

        Assert.AreEqual(expected, row);
    }


    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("1,2,3|4,5,7", (short)0, "1,4")]
    [DataRow("1,2,3|4,5,7", (short)1, "2,5")]
    [DataRow("1,2,3|4,5,7", (short)2, "3,7")]
    public void ColumnTest(string input, short columnIndex, string expectedString)
    {
        var matrix = SqlMatrixF.Parse(input);
        this.TestContext.WriteLine("matrix:");
        this.TestContext.WriteLine(matrix.ToString());

        var column = matrix.Column(columnIndex);
        this.TestContext.WriteLine("column:");
        this.TestContext.WriteLine(column.ToString());

        var expected = SqlVectorF.Parse(expectedString);

        Assert.AreEqual(expected, column);
    }
}
