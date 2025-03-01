using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.ComponentModel;
using OoBDev.TestUtilities;
using System;
using System.Collections;

namespace OoBDev.System.Tests.ComponentModel;

[TestClass]
public class DataConverterTests
{
    public required TestContext TestContext { get; set; }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("1", 1)]
    [DataRow("1", 1L)]
    [DataRow("1", 1d)]
    [DataRow("1", 1ul)]
    [DataRow(1, "1")]
    [DataRow(1L, "1")]
    [DataRow(1d, "1")]
    [DataRow(1ul, "1")]
    [DataRow("1", new[] { 1 })]
    [DataRow("1,2", new[] { 1, 2 })]
    [DataRow("[1,2]", new[] { 1, 2 })]
    [DataRow("[\"1\",\"2\"]", new[] { 1, 2 })]
    [DataRow("['1','2']", new[] { 1, 2 })]
    [DataRow("['OH','MI']", new[] { "OH", "MI" })]
    [DataRow("[\"OH\",\"MI\"]", new[] { "OH", "MI" })]
    public void ConvertToTest(object input, object expected)
    {
        var converter = new DataConverter();
        var result = converter.ConvertTo(input, expected.GetType());

        if (expected is ICollection expectedCollection && result is ICollection resultCollection)
        {
            CollectionAssert.AreEqual(expectedCollection, resultCollection);
        }
        else
        {
            Assert.AreEqual(expected, result);
        }
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("+05:30", typeof(TimeSpan), "05:30:00")]
    [DataRow("-05:30", typeof(TimeSpan), "-05:30:00")]
    [DataRow("05:30", typeof(TimeSpan), "05:30:00")]
    public void ConvertToTest(object input, Type targetType, string expected)
    {
        var converter = new DataConverter();
        var result = converter.ConvertTo(input, targetType);

        Assert.IsNotNull(result);
        Assert.AreEqual(expected, result.ToString());
    }
}
