using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Docs.Tests;

[TestClass]
public class ScreenshotFreshnessTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly DateTimeOffset Day = new(2026, 11, 16, 0, 0, 0, TimeSpan.Zero);

    private static readonly Dictionary<string, string[]> Sources = new() { ["wpf-"] = ["wpf"], ["tui-"] = ["tui"] };

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FindStale_CodeNewerThanTolerance_ReportsTheImage()
    {
        var images = new Dictionary<string, DateTimeOffset> { ["wpf-main.png"] = Day.AddDays(-40), ["tui-home.png"] = Day.AddDays(-40) };

        var stale = ScreenshotFreshness.FindStale(images, Sources, s => s[0] == "wpf" ? Day : Day.AddDays(-35), TimeSpan.FromDays(30)).ToList();

        Assert.HasCount(1, stale);
        StringAssert.StartsWith(stale[0], "wpf-main.png");
        StringAssert.Contains(stale[0], "40 days newer");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FindStale_ImageNewerThanCode_IsFresh()
    {
        var images = new Dictionary<string, DateTimeOffset> { ["wpf-main.png"] = Day };

        Assert.IsEmpty(ScreenshotFreshness.FindStale(images, Sources, _ => Day.AddDays(-5), TimeSpan.Zero).ToList());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FindStale_UnknownPrefix_IsReported()
    {
        var images = new Dictionary<string, DateTimeOffset> { ["cli-run.png"] = Day };

        var stale = ScreenshotFreshness.FindStale(images, Sources, _ => Day, TimeSpan.Zero).ToList();

        Assert.HasCount(1, stale);
        StringAssert.Contains(stale[0], "no source folders");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FindStale_AsksForEachPrefixOnce()
    {
        var images = new Dictionary<string, DateTimeOffset> { ["wpf-a.png"] = Day, ["wpf-b.png"] = Day };
        var calls = 0;

        _ = ScreenshotFreshness.FindStale(images, Sources, _ => { calls++; return Day; }, TimeSpan.Zero).ToList();

        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GitDate_ThisRepository_ReturnsADate()
    {
        var date = ScreenshotFreshness.GitDate(RepoRoot(), "CLAUDE.md");

        Assert.IsNotNull(date);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Screenshots_AreNotOlderThanTheirCode()
    {
        if (!string.Equals(TestContext.GetPropertyOrDefault("DOCS_CHECK_SCREENSHOTS", "false"), "true", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Inconclusive("Set DOCS_CHECK_SCREENSHOTS=true to compare screenshot dates with the code they show (needs git history).");
        }

        var root = RepoRoot();
        var folder = "docs/applications/devterm/user-guide/images";
        var sources = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(Path.Combine(root, folder, "sources.json")))!;
        var images = Directory.GetFiles(Path.Combine(root, folder), "*.png")
            .Select(f => Path.GetFileName(f))
            .ToDictionary(n => n, n => ScreenshotFreshness.GitDate(root, $"{folder}/{n}") ?? DateTimeOffset.MaxValue);
        var days = int.Parse(TestContext.GetPropertyOrDefault("DOCS_SCREENSHOT_TOLERANCE_DAYS", "30"), CultureInfo.InvariantCulture);

        var stale = ScreenshotFreshness.FindStale(images, sources, s => ScreenshotFreshness.GitDate(root, s), TimeSpan.FromDays(days)).ToList();

        Assert.IsEmpty(stale, string.Join(Environment.NewLine, stale));
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CLAUDE.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root (CLAUDE.md) not found.");
    }
}
