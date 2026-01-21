using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.ComponentModel.Validations;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.ComponentModel.Validations;

[TestClass]
public class ZipCodeAttributeTests
{
    public required TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("12345", true)]
    [DataRow("12345-1111", true)]
    [DataRow("123451111", false)]
    [DataRow("a1234", false)]
    [DataRow("12345,12345", false)]
    [DataRow("12345,12345-1234", false)]
    [DataRow("12345,12345-1234,12345", false)]
    [DataRow("12345,12345-1234,abc", false)]
    public void IsValidTest(string input, bool expected)
    {
        var validation = new ZipCodeAttribute();
        var result = validation.IsValid(input);
        TestContext.WriteLine($"IsValid(\"{input}\") -> {result} == {expected}");
        Assert.AreEqual(expected, result);
    }
}
