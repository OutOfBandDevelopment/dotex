using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.AI;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.LlmCodeGen.Cli.Tests;

[TestClass]
public class FolderPromptRunnerTests
{
    private string _root = string.Empty;
    private string _template = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _root = Path.Combine(Path.GetTempPath(), "llmcodegen-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_root, "in", "sub"));
        File.WriteAllText(Path.Combine(_root, "in", "a.txt"), "alpha");
        File.WriteAllText(Path.Combine(_root, "in", "sub", "b.txt"), "beta");
        _template = Path.Combine(_root, "t.hbs");
        File.WriteAllText(_template, "{{folder}}:{{#each files}}[{{name}}={{content}}]{{/each}}");
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_root, true);

    private LlmCodeGenOptions Options(bool overwrite = false, bool extract = true) => new()
    {
        InputPath = Path.Combine(_root, "in"),
        OutputPath = Path.Combine(_root, "out"),
        Template = _template,
        Model = "m",
        Overwrite = overwrite,
        ExtractFiles = extract,
    };

    private static Mock<IMessageCompletion> Completion(string response)
    {
        var mock = new Mock<IMessageCompletion>();
        mock.Setup(c => c.GetCompletionAsync("m", It.IsAny<string>())).ReturnsAsync(response);
        return mock;
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task RunAsync_WritesPromptResponseAndExtractedFilesPerFolder()
    {
        // Stage
        var completion = Completion("**gen.txt**\n```\nmade\n```");
        var runner = new FolderPromptRunner(completion.Object, NullLogger<FolderPromptRunner>.Instance);

        // Test
        var sent = await runner.RunAsync(Options(), CancellationToken.None);

        // Assert
        Assert.AreEqual(2, sent);
        var outRoot = Path.Combine(_root, "out");
        Assert.AreEqual("in:[a.txt=alpha]", File.ReadAllText(Path.Combine(outRoot, "t.prompt.md")));
        Assert.AreEqual("sub:[b.txt=beta]", File.ReadAllText(Path.Combine(outRoot, "sub", "t.prompt.md")));
        Assert.IsTrue(File.Exists(Path.Combine(outRoot, "t.response.md")));
        Assert.AreEqual("made", File.ReadAllText(Path.Combine(outRoot, "sub", "gen.txt")));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task RunAsync_ExistingResponse_SkippedUnlessOverwrite()
    {
        // Stage
        var completion = Completion("ok");
        var runner = new FolderPromptRunner(completion.Object, NullLogger<FolderPromptRunner>.Instance);
        await runner.RunAsync(Options(), CancellationToken.None);

        // Test
        var skipped = await runner.RunAsync(Options(), CancellationToken.None);
        var rerun = await runner.RunAsync(Options(overwrite: true), CancellationToken.None);

        // Assert
        Assert.AreEqual(0, skipped);
        Assert.AreEqual(2, rerun);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task RunAsync_ExtractDisabled_WritesNoExtractedFiles()
    {
        // Stage
        var runner = new FolderPromptRunner(Completion("**gen.txt**\n```\nmade\n```").Object, NullLogger<FolderPromptRunner>.Instance);

        // Test
        await runner.RunAsync(Options(extract: false), CancellationToken.None);

        // Assert
        Assert.IsFalse(File.Exists(Path.Combine(_root, "out", "gen.txt")));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task RunAsync_TraversalNameInResponse_StaysInsideOutput()
    {
        // Stage
        var runner = new FolderPromptRunner(Completion("**../../escape.txt**\n```\nx\n```").Object, NullLogger<FolderPromptRunner>.Instance);

        // Test
        await runner.RunAsync(Options(), CancellationToken.None);

        // Assert
        Assert.IsFalse(File.Exists(Path.Combine(_root, "escape.txt")));
        Assert.IsTrue(File.Exists(Path.Combine(_root, "out", "error1.txt")));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task RunAsync_Cancelled_Stops()
    {
        // Stage
        var runner = new FolderPromptRunner(Completion("ok").Object, NullLogger<FolderPromptRunner>.Instance);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Test / Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => runner.RunAsync(Options(), cts.Token));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Load_UnknownTemplate_Throws()
    {
        // Test / Assert
        Assert.Throws<FileNotFoundException>(() => PromptRenderer.Load("does-not-exist"));
    }
}
