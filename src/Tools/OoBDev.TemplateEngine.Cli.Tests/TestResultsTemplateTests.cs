using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.IO;
using OoBDev.System.Text.Templating;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.TemplateEngine.Cli.Tests;

/// <summary>The <c>TestResultsToMarkdown.md.xslt</c> template turns a .trx file into the markdown summary.</summary>
[TestClass]
[DoNotParallelize]
public sealed class TestResultsTemplateTests
{
    private const string Trx = """
        <?xml version="1.0" encoding="utf-8"?>
        <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
          <Results>
            <UnitTestResult testId="1" testName="Adds" outcome="Passed" />
            <UnitTestResult testId="2" testName="Breaks" outcome="Failed">
              <Output><ErrorInfo><Message>Expected 1
        but was 2</Message></ErrorInfo></Output>
            </UnitTestResult>
            <UnitTestResult testId="3" testName="NoDevice" outcome="Inconclusive">
              <Output><ErrorInfo><Message>Radex One not detected</Message></ErrorInfo></Output>
            </UnitTestResult>
            <UnitTestResult testId="4" testName="Ignored" outcome="NotExecuted" />
            <UnitTestResult testId="5" testName="Talks" outcome="Passed" />
          </Results>
          <TestDefinitions>
            <UnitTest id="1" name="Adds"><TestCategory><TestCategoryItem TestCategory="Unit" /></TestCategory><TestMethod className="A.MathTests" name="Adds" /></UnitTest>
            <UnitTest id="2" name="Breaks"><TestCategory><TestCategoryItem TestCategory="Unit" /></TestCategory><TestMethod className="A.MathTests" name="Breaks" /></UnitTest>
            <UnitTest id="3" name="NoDevice"><TestCategory><TestCategoryItem TestCategory="Hardware" /></TestCategory><TestMethod className="A.RadexTests" name="NoDevice" /></UnitTest>
            <UnitTest id="4" name="Ignored"><TestCategory><TestCategoryItem TestCategory="Unit" /></TestCategory><TestMethod className="A.MathTests" name="Ignored" /></UnitTest>
            <UnitTest id="5" name="Talks"><TestCategory><TestCategoryItem TestCategory="Integration" /></TestCategory><TestMethod className="A.BusTests" name="Talks" /></UnitTest>
          </TestDefinitions>
        </TestRun>
        """;

    [TestMethod, TestCategory("Unit")]
    public async Task Template_TrxWithAllOutcomes_ReportsTotalsTiersAndLists()
    {
        using var temp = new TempFileFactory(new ServiceCollection().BuildServiceProvider()).GetTempDirectory();
        File.WriteAllText(Path.Combine(temp.DirectoryPath, "run.trx"), Trx);

        await new TemplateEngineService(
            NullLogger<TemplateEngineService>.Instance,
            Options.Create(new TemplateEngineOptions
            {
                InputFile = Path.Combine(temp.DirectoryPath, "run.trx"),
                Template = Path.Combine(FindTemplates(), "TestResultsToMarkdown.md.xslt"),
                OutputFile = Path.Combine(temp.DirectoryPath, "[file].md"),
            }),
            new Mock<ITemplateEngine>(MockBehavior.Strict).Object).StartAsync(CancellationToken.None);

        var report = File.ReadAllText(Path.Combine(temp.DirectoryPath, "run.md")).Replace("\r\n", "\n", StringComparison.Ordinal);
        StringAssert.Contains(report, "| Total | 5 |\n| Passed | 2 |\n| Failed | 1 |\n| Inconclusive | 1 |\n| Skipped | 1 |");
        StringAssert.Contains(report, "| Unit | 3 | 1 | 1 | 0 | 1 |");
        StringAssert.Contains(report, "| Hardware | 1 | 0 | 0 | 1 | 0 |");
        StringAssert.Contains(report, "| Integration | 1 | 1 | 0 | 0 | 0 |");
        StringAssert.Contains(report, "## Failed\n\n- `A.MathTests.Breaks` — Expected 1\n");
        StringAssert.Contains(report, "## Inconclusive\n\n- `A.RadexTests.NoDevice` — Radex One not detected\n");
        StringAssert.Contains(report, "## Skipped\n\n- `A.MathTests.Ignored`\n");
        Assert.IsFalse(report.Contains("| Simulate |", StringComparison.Ordinal));
    }

    private static string FindTemplates()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "docs", "templates");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException("docs/templates was not found above the test output.");
    }
}
