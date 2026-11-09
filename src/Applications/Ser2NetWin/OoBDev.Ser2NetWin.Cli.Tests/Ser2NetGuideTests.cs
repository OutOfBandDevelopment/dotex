using OoBDev.Documents.Guides;
using OoBDev.TestUtilities;

namespace OoBDev.Ser2NetWin.Cli.Tests;

/// <summary>Generates the command guide into <c>docs/applications/ser2netwin/user-guide.md</c> from a real run of the built tool (design: docs/design/GeneratedGuides).</summary>
[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class Ser2NetGuideTests
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

    [TestMethod]
    public async Task Help_WritesTheGuideAsync()
    {
        // Stage
        var dll = Path.Combine(AppContext.BaseDirectory, "ser2net-win.dll");

        // Test
        var help = await CliToolRunner.RunAsync(
            "Commands and options",
            "Run the tool with `--help` to list its commands and options.",
            "dotnet",
            [dll, "--help"],
            "ser2net-win --help",
            AppContext.BaseDirectory,
            TestContext.CancellationToken);
        var guide = CliToolGuide.Render("ser2net-win", "Bridges Windows COM ports to raw TCP or RFC 2217.", [help]);
        ApiGuideRenderer.WriteIfChanged(Path.Combine(RepoRoot(), "docs", "applications", "ser2netwin", "user-guide.md"), guide);

        // Assert
        Assert.AreEqual(0, help.ExitCode);
        StringAssert.Contains(guide, "--config <path>");
    }
}
