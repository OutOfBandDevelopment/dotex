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

    /// <summary>The tool's own build output: it holds every package the tool needs, which the test's bin folder lacks for packages that come from the shared ASP.NET Core framework.</summary>
    private static string ToolDirectory(string assembly)
    {
        var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var framework = baseDirectory.Name;
        var configuration = baseDirectory.Parent!.Name;
        return Path.Combine(RepoRoot(), "src", "Tools", assembly, "bin", configuration, framework);
    }

    private Task<CliExample> RunAsync(string assembly, string toolName, string title, string description, params string[] arguments)
    {
        var toolDirectory = ToolDirectory(assembly);
        var dll = Path.Combine(toolDirectory, assembly + ".dll");
        var all = new List<string> { dll };
        all.AddRange(arguments);
        var shown = toolName + (arguments.Length == 0 ? string.Empty : " " + string.Join(' ', arguments));
        return CliToolRunner.RunAsync(title, description, "dotnet", all, shown, toolDirectory, TestContext.CancellationToken);
    }

    private static void Write(string tool, string guide) =>
        ApiGuideRenderer.WriteIfChanged(Path.Combine(RepoRoot(), "docs", "applications", "tools", tool + ".md"), guide);

    private async Task HelpGuideAsync(string assembly, string toolName, string synopsis, string mustContain)
    {
        // Stage
        var help = await RunAsync(assembly, toolName, "Options", "Run the tool with `--help` to list its options.", "--help");

        // Test
        var guide = CliToolGuide.Render(toolName, synopsis, [help]);
        Write(toolName, guide);

        // Assert
        Assert.AreEqual(0, help.ExitCode);
        StringAssert.Contains(guide, mustContain);
    }

    [TestMethod]
    public Task TemplateEngine_Help_WritesTheGuideAsync() =>
        HelpGuideAsync("OoBDev.TemplateEngine.Cli", "oobdev-templateengine", "Renders a Handlebars or XSLT template over input files.", "--merge");

    [TestMethod]
    public Task DocumentConverter_Help_WritesTheGuideAsync() =>
        HelpGuideAsync("OoBDev.DocumentConverter.Cli", "oobdev-documentconverter", "Converts a document from one format to another.", "--output");

    [TestMethod]
    public Task FileRagEngine_Help_WritesTheGuideAsync() =>
        HelpGuideAsync("OoBDev.FileRagEngine.Cli", "oobdev-filerag", "Runs a template over files with a language model.", "--include-prompt");

    [TestMethod]
    public Task DacPacCompiler_Help_WritesTheGuideAsync() =>
        HelpGuideAsync("OoBDev.DacPacCompiler.Cli", "oobdev-dacpac", "Builds a DACPAC from a SQL CLR assembly.", "--sqlclr");

    [TestMethod]
    public Task FixSourceLinks_Help_WritesTheGuideAsync() =>
        HelpGuideAsync("OoBDev.FixSourceLinks.Cli", "oobdev-fixsourcelinks", "Rewrites https links in XML reports to local paths.", "--include");

    [TestMethod]
    public Task MigrationHelper_Help_WritesTheGuideAsync() =>
        HelpGuideAsync("OoBDev.MigrationHelper.Cli", "oobdev-migrationhelper", "Renames a project prefix in folder names, file names and file contents.", "--path");

    [TestMethod]
    public Task LlmCodeGen_Help_WritesTheGuideAsync() =>
        HelpGuideAsync("OoBDev.LlmCodeGen.Cli", "oobdev-llmcodegen", "Sends the files of each folder to a language model with a template and writes the responses.", "--provider");

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
