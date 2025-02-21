using OoBDev.Extensions.Linq;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace OoBDev.Extensions.Tests.Linq;

[TestClass]
public class DictionaryExtensionsTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryGetValueTest()
    {
        var dict = new Dictionary<string, string>()
        {
            {"HELLO", "world" },
        };

        Assert.IsTrue(dict.TryGetValue("hello", out var value, StringComparer.InvariantCultureIgnoreCase));
        Assert.AreEqual("world", value);
    }
}
