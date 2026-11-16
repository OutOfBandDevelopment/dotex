using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using System;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.Text.Templating;

[TestClass]
public class TemplateRenderHarnessTests
{
    private static Mock<ITemplateEngine> Engine(string name, string? output, bool registered = true)
    {
        var engine = new Mock<ITemplateEngine>();
        engine.Setup(e => e.Get(name)).Returns(registered ? new Mock<ITemplateContext>().Object : null);
        engine.Setup(e => e.ApplyAsync(name, It.IsAny<object>())).ReturnsAsync(output);
        return engine;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task CheckAsync_MatchingOutput_ReturnsNoFailures()
    {
        var engine = Engine("welcome", "Hello Ada\r\nBye\r\n");

        var failures = await TemplateRenderHarness.CheckAsync(engine.Object, [new("welcome", new { }, "Hello Ada\nBye")]);

        Assert.IsEmpty(failures);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task CheckAsync_DifferentLine_NamesTheFirstDifference()
    {
        var engine = Engine("welcome", "Hello Bob\nBye");

        var failures = await TemplateRenderHarness.CheckAsync(engine.Object, [new("welcome", new { }, "Hello Ada\nBye")]);

        Assert.HasCount(1, failures);
        StringAssert.Contains(failures[0].Reason, "Line 1");
        StringAssert.Contains(failures[0].Reason, "Hello Bob");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task CheckAsync_ExtraLine_IsReportedAgainstEnd()
    {
        var engine = Engine("welcome", "a\nb\nc");

        var failures = await TemplateRenderHarness.CheckAsync(engine.Object, [new("welcome", new { }, "a\nb")]);

        StringAssert.Contains(failures[0].Reason, "<end>");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task CheckAsync_MissingTemplate_IsAFailure()
    {
        var engine = Engine("gone", "x", registered: false);

        var failures = await TemplateRenderHarness.CheckAsync(engine.Object, [new("gone", new { }, "x")]);

        StringAssert.Contains(failures[0].Reason, "No template");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task CheckAsync_NothingRendered_IsAFailure()
    {
        var engine = Engine("empty", null);

        var failures = await TemplateRenderHarness.CheckAsync(engine.Object, [new("empty", new { }, "x")]);

        StringAssert.Contains(failures[0].Reason, "rendered nothing");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task CheckAsync_EngineThrows_IsAFailureNotAnException()
    {
        var engine = new Mock<ITemplateEngine>();
        engine.Setup(e => e.Get("boom")).Returns(new Mock<ITemplateContext>().Object);
        engine.Setup(e => e.ApplyAsync("boom", It.IsAny<object>())).ThrowsAsync(new InvalidOperationException("bad data"));

        var failures = await TemplateRenderHarness.CheckAsync(engine.Object, [new("boom", new { }, "x")]);

        StringAssert.Contains(failures[0].Reason, "bad data");
    }
}
