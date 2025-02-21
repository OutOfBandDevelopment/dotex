using OoBDev.Extensions.Configuration;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace OoBDev.Extensions.Tests.Configuration;

[TestClass]
public class CommandLineTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void BuildParametersTests()
    {
        var dict = CommandLine.BuildParameters<TestHarness>();

        Assert.IsTrue(dict.TryGetValue($"--{nameof(TestHarness.Prop1)}", out var prop1));
        Assert.IsTrue(dict.TryGetValue($"--{nameof(TestHarness.Prop2)}", out var prop2));

        Assert.AreEqual($"{nameof(TestHarness)}:{nameof(TestHarness.Prop1)}", prop1);
        Assert.AreEqual($"{nameof(TestHarness)}:{nameof(TestHarness.Prop2)}", prop2);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddParametersTests()
    {
        var dict = new Dictionary<string, string>().AddParameters<TestHarness>();

        Assert.IsTrue(dict.TryGetValue($"--{nameof(TestHarness.Prop1)}", out var prop1));
        Assert.IsTrue(dict.TryGetValue($"--{nameof(TestHarness.Prop2)}", out var prop2));

        Assert.AreEqual($"{nameof(TestHarness)}:{nameof(TestHarness.Prop1)}", prop1);
        Assert.AreEqual($"{nameof(TestHarness)}:{nameof(TestHarness.Prop2)}", prop2);
    }

    public class TestHarness
    {
        public string? Prop1 { get; set; }
        public string? Prop2 { get; set; }
    }
}
