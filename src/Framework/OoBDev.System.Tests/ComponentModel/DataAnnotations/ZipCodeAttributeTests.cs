using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.ComponentModel.DataAnnotations;

namespace OoBDev.System.Tests.ComponentModel.DataAnnotations;

[TestClass]
public class ZipCodeAttributeTests
{
    public TestContext TestContext { get; set; } = null!;

    [DataTestMethod]
    [TestCategory("UNIT")]
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
