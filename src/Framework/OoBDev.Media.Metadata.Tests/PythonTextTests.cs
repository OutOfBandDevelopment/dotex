using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
public class PythonTextTests
{
    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(0.0, "0.0")]
    [DataRow(1.0, "1.0")]
    [DataRow(-2.0, "-2.0")]
    [DataRow(0.5, "0.5")]
    [DataRow(0.1, "0.1")]
    [DataRow(123.456, "123.456")]
    [DataRow(100.0, "100.0")]
    [DataRow(0.0001, "0.0001")]
    [DataRow(0.00001, "1e-05")]
    [DataRow(1e16, "1e+16")]
    [DataRow(1.5e16, "1.5e+16")]
    [DataRow(1e15, "1000000000000000.0")]
    [DataRow(1.0 / 3, "0.3333333333333333")]
    public void Float_MatchesPythonRepr(double value, string expected)
    {
        Assert.AreEqual(expected, PythonText.Float(value));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Float_SpecialValues()
    {
        Assert.AreEqual("nan", PythonText.Float(double.NaN));
        Assert.AreEqual("inf", PythonText.Float(double.PositiveInfinity));
        Assert.AreEqual("-inf", PythonText.Float(double.NegativeInfinity));
        Assert.AreEqual("-0.0", PythonText.Float(-0.0));
        Assert.AreEqual("-1.5e-07", PythonText.Float(-1.5e-7));
        Assert.AreEqual("1.2345e+20", PythonText.Float(1.2345e20));
        Assert.IsTrue(PythonText.Float(double.Epsilon).Contains("e-324", StringComparison.Ordinal));
    }
}
