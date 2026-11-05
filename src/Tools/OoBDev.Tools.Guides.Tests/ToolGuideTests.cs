using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Guides;
using OoBDev.TestUtilities;

namespace OoBDev.Tools.Guides.Tests;

/// <summary>Generates the command-line tool guides into <c>docs/applications/tools</c> from real runs of the built tools (design: docs/design/GeneratedGuides).</summary>
[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class ToolGuideTests
{
    public TestContext TestContext { get; set; } = null!;

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GitVersion.yml")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }

    private Task<CliExample> RunAsync(string assembly, string toolName, string title, string description, params string[] arguments)
    {
        var dll = Path.Combine(AppContext.BaseDirectory, assembly + ".dll");
        var all = new List<string> { dll };
        all.AddRange(arguments);
        var shown = toolName + (arguments.Length == 0 ? string.Empty : " " + string.Join(' ', arguments));
        return CliToolRunner.RunAsync(title, description, "dotnet", all, shown, AppContext.BaseDirectory, TestContext.CancellationToken);
    }

    private static void Write(string tool, string guide) =>
        ApiGuideRenderer.WriteIfChanged(Path.Combine(RepoRoot(), "docs", "applications", "tools", tool + ".md"), guide);

    [TestMethod]
    public async Task Xslt_Help_WritesTheGuideAsync()
    {
        // Stage
        var help = await RunAsync("OoBDev.Xslt.Cli", "oobdev-xslt", "Options", "Run the tool with `--help` to list its options.", "--help");

        // Test
        var guide = CliToolGuide.Render("oobdev-xslt", "Applies an XSLT template to XML, HTML, JSON, YAML, C#, VB, MSBuild structured logs, DACPACs or paths.", [help]);
        Write("oobdev-xslt", guide);

        // Assert
        Assert.AreEqual(0, help.ExitCode);
        StringAssert.Contains(guide, "--template");
    }

    [TestMethod]
    public async Task PackMan_Help_WritesTheGuideAsync()
    {
        // Stage
        var help = await RunAsync("OoBDev.PackMan.Cli", "oobdev-packman", "Options", "Run the tool with `--help` to list its options.", "--help");

        // Test
        var guide = CliToolGuide.Render("oobdev-packman", "Maintains the central package versions in `Directory.Packages.props`.", [help]);
        Write("oobdev-packman", guide);

        // Assert
        Assert.AreEqual(0, help.ExitCode);
        StringAssert.Contains(guide, "--update-projects");
    }

    [TestMethod]
    public async Task ImageMask_NoArguments_WritesTheUsageGuideAsync()
    {
        // Stage
        var usage = await RunAsync("OoBDev.ImageMask.Cli", "image-mask", "Usage", "Without arguments the tool prints its commands and exits with code 2.");

        // Test
        var guide = CliToolGuide.Render("image-mask", "Masks volatile zones in guide screenshots and compares images so a committed image changes only when the picture does.", [usage]);
        Write("image-mask", guide);

        // Assert
        Assert.AreEqual(2, usage.ExitCode);
        StringAssert.Contains(guide, "image-mask sync");
    }
}
