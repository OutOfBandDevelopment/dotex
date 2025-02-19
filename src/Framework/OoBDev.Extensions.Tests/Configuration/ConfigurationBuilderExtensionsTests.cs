using OoBDev.Extensions.Configuration;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace OoBDev.Extensions.Tests.Configuration;

[TestClass]
public class ConfigurationBuilderExtensionsTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddInMemoryCollectionTest_KeyValuePair()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hello"] = "world",
            })
            .Build();

        var result = config["Hello"];

        Assert.AreEqual("world", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddInMemoryCollectionTest_Tuple()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(("Hello", "world"))
            .Build();

        var result = config["Hello"];

        Assert.AreEqual("world", result);
    }
}
