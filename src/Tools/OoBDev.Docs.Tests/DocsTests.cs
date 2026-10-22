using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Docs.Tests;

[TestClass]
public class DocsTests
{
    public TestContext TestContext { get; set; } = null!;

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CLAUDE.md")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? throw new InvalidOperationException("Repository root (CLAUDE.md) not found.");
    }

    private IReadOnlyList<MarkdownDocument> Documents(string root)
    {
        var configured = TestContext.GetPropertyOrDefault("DOCS_VALIDATE_PATHS", "docs/patterns-discovery");
        return [.. configured
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .SelectMany(p => MarkdownDocument.Find(root, p))
            .Order(StringComparer.Ordinal)
            .Select(MarkdownDocument.Parse)];
    }

    private static void AssertNoProblems(IEnumerable<string> problems)
    {
        var list = problems.ToList();
        Assert.AreEqual(0, list.Count, string.Join(Environment.NewLine, list));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Docs_ConfiguredPaths_ContainMarkdown()
    {
        // Stage
        var root = RepoRoot();

        // Test
        var documents = Documents(root);

        // Assert
        Assert.IsNotEmpty(documents, "DOCS_VALIDATE_PATHS matched no markdown files.");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void PlantUml_Blocks_AreWellFormed()
    {
        var root = RepoRoot();
        AssertNoProblems(Documents(root).SelectMany(d => DocsValidator.CheckPlantUml(root, d)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Links_Relative_Resolve()
    {
        var root = RepoRoot();
        AssertNoProblems(Documents(root).SelectMany(d => DocsValidator.CheckLinks(root, d)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Captions_FiguresAndTables_AreSequential()
    {
        var root = RepoRoot();
        AssertNoProblems(Documents(root).SelectMany(d => DocsValidator.CheckCaptions(root, d)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task PlantUml_Blocks_Render()
    {
        // Stage
        var root = RepoRoot();
        var baseUrl = TestContext.GetPropertyOrDefault("PLANTUML_URL", "http://localhost:8086").TrimEnd('/');
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        var problems = new List<string>();

        // Test
        foreach (var document in Documents(root))
        {
            foreach (var block in document.Blocks)
            {
                var encoded = "~h" + Convert.ToHexString(Encoding.UTF8.GetBytes(block.Text)).ToLowerInvariant();
                using var response = await client.GetAsync($"{baseUrl}/svg/{encoded}", TestContext.CancellationToken);
                var svg = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);
                if (!response.IsSuccessStatusCode || svg.Contains("Syntax Error", StringComparison.Ordinal))
                {
                    problems.Add($"{Path.GetRelativePath(root, document.Path)}:{block.Line}: render failed (HTTP {(int)response.StatusCode})");
                }
            }
        }

        // Assert
        AssertNoProblems(problems);
    }
}
