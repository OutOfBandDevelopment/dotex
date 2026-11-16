using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class FilterScalarTests
{
    private static object? Scalar(string json) => FilterCompiler.Scalar(null, JsonDocument.Parse(json).RootElement);

    [TestMethod]
    public void Scalar_WholeNumber_IsLong() => Assert.IsInstanceOfType<long>(Scalar("5"));

    [TestMethod]
    public void Scalar_Fraction_IsDouble() => Assert.AreEqual(2.5, Scalar("2.5"));
}
