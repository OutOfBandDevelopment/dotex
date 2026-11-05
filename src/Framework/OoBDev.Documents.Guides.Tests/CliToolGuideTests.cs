using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Guides.Tests;

[TestCategory(TestCategories.Unit)]
[TestClass]
public sealed class CliToolGuideTests
{
    [TestMethod]
    public void Render_TwoExamples_WritesHeadingsAndConsoleBlocks()
    {
        // Stage
        var examples = new List<CliExample>
        {
            new("Show help", "Lists the options.", "tool --help", "Usage: tool\r\n", 0),
            new("Bad input", string.Empty, "tool nope", "error: nope", 2),
        };

        // Test
        var guide = CliToolGuide.Render("tool guide", "Does things.", examples);

        // Assert
        StringAssert.StartsWith(guide, "# tool guide\n\nDoes things.\n");
        StringAssert.Contains(guide, "## Show help\n\nLists the options.\n\n```console\n$ tool --help\nUsage: tool\n```\n");
        StringAssert.Contains(guide, "```\n\nExit code: `2`\n");
        Assert.DoesNotContain("\r", guide);
    }

    [TestMethod]
    public void Render_SameExamples_SameBytes()
    {
        // Stage
        var examples = new List<CliExample> { new("A", "d", "tool a", "out", 0) };

        // Test
        var first = CliToolGuide.Render("t", "s", examples);
        var second = CliToolGuide.Render("t", "s", examples);

        // Assert
        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void Render_BlankTitle_Throws()
    {
        // Stage

        // Test
        var act = () => CliToolGuide.Render(" ", string.Empty, Array.Empty<CliExample>());

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [TestMethod]
    public async Task RunAsync_DotnetVersion_CapturesOutputAndExitCodeAsync()
    {
        // Stage

        // Test
        var example = await CliToolRunner.RunAsync("Version", string.Empty, "dotnet", new[] { "--version" }, "dotnet --version");

        // Assert
        Assert.AreEqual(0, example.ExitCode);
        Assert.IsGreaterThan(0, example.Output.Trim().Length);
        Assert.AreEqual("dotnet --version", example.CommandLine);
    }
}
