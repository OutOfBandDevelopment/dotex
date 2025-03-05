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
}
