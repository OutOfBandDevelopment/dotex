using OoBDev.System.Providers;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OoBDev.System.Tests.Providers;

[TestClass]
public class GuidProviderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void NewGuidTest()
    {
        var provider = new GuidProvider();
        var value = provider.NewGuid();
        Assert.AreNotEqual(Guid.Empty, value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EmptyTest()
    {
        var provider = new GuidProvider();
        var value = provider.Empty;
        Assert.AreEqual(Guid.Empty, value);
    }

}
