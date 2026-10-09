using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OoBDev.TestUtilities.Tests;

[TestClass]
public class TestContextPropertyTests
{
    public required TestContext TestContext { get; set; }

    private const string Missing = "OOBDEV_TEST_PROPERTY_THAT_DOES_NOT_EXIST";
    private const string FromEnvironment = "OOBDEV_TEST_PROPERTY_FROM_ENVIRONMENT";

    [TestCleanup]
    public void Cleanup() => Environment.SetEnvironmentVariable(FromEnvironment, null);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetPropertyOrDefault_MissingValueType_ReturnsSuppliedDefault()
    {
        Assert.AreEqual(6334, TestContext.GetPropertyOrDefault(Missing, 6334));
        Assert.IsTrue(TestContext.GetPropertyOrDefault(Missing, true));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetPropertyOrDefault_MissingReferenceType_ReturnsSuppliedDefault() =>
        Assert.AreEqual("fallback", TestContext.GetPropertyOrDefault(Missing, "fallback"));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetPropertyOrDefault_EnvironmentValue_IsConverted()
    {
        Environment.SetEnvironmentVariable(FromEnvironment, "42");

        Assert.AreEqual(42, TestContext.GetPropertyOrDefault(FromEnvironment, 7));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetPropertyOrDefault_ExplicitZero_IsNotReplacedByDefault()
    {
        Environment.SetEnvironmentVariable(FromEnvironment, "0");

        Assert.AreEqual(0, TestContext.GetPropertyOrDefault(FromEnvironment, 7));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetProperty_Missing_ReturnsNullForReferenceTypes() =>
        Assert.IsNull(TestContext.GetProperty<string>(Missing));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetRequiredProperty_Missing_Throws() =>
        Assert.Throws<ApplicationException>(() => TestContext.GetRequiredProperty<int>(Missing));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryGetProperty_MissingAndPresent_ReportsFound()
    {
        Environment.SetEnvironmentVariable(FromEnvironment, "5");

        Assert.IsFalse(TestContext.TryGetProperty<int>(Missing, out _));
        Assert.IsTrue(TestContext.TryGetProperty<int>(FromEnvironment, out var value));
        Assert.AreEqual(5, value);
    }
}
