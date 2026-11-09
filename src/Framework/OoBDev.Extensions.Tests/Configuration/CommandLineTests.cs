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

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Usage_OptionType_ListsSwitchesTypesAndDescriptions()
    {
        // Stage

        // Test
        var usage = CommandLine.Usage("demo", "Does demo things.", typeof(UsageHarness));

        // Assert
        StringAssert.StartsWith(usage, "demo - Does demo things.\n");
        StringAssert.Contains(usage, "--input  <string> Where to read from\n");
        StringAssert.Contains(usage, "--Mode   <Fast|Slow>\n");
        StringAssert.Contains(usage, "--Name   <string> (required)\n");
        StringAssert.Contains(usage, "--raw    <true|false>\n");
        Assert.DoesNotContain("\r", usage);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void IsHelpRequest_HelpFlags_AreRecognised()
    {
        Assert.IsTrue(CommandLine.IsHelpRequest(["--input", "a", "--help"]));
        Assert.IsTrue(CommandLine.IsHelpRequest(["-?"]));
        Assert.IsFalse(CommandLine.IsHelpRequest(["--input", "a"]));
    }

    public enum UsageMode
    {
        Fast,
        Slow,
    }

    public class UsageHarness
    {
        [global::System.ComponentModel.Description("Where to read from")]
        [OoBDev.System.Configuration.CommandParameter(Value = "input")]
        public string? InputPath { get; set; }

        public UsageMode? Mode { get; set; }

        [global::System.ComponentModel.DataAnnotations.Required]
        public string? Name { get; set; }

        [OoBDev.System.Configuration.CommandParameter(Value = "raw")]
        public bool Raw { get; set; }
    }

    public class TestHarness
    {
        public string? Prop1 { get; set; }
        public string? Prop2 { get; set; }
    }
}
