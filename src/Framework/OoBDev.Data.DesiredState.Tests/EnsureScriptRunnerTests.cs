using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public sealed class EnsureScriptRunnerTests
{
    private static readonly DatabaseTarget _target = new("h", 1, "db", null, null);

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RunAsync_IdempotentScripts_RunInNameOrderEveryTime()
    {
        // Stage
        var executor = new Recorder();
        var runner = new EnsureScriptRunner(executor, new InMemoryAppliedScriptStore());
        var scripts = new DesiredSchema([new DesiredScript("020_queues.sql", "B"), new DesiredScript("001_ext.sql", "A")]);

        // Test
        await runner.RunAsync(scripts, _target, TestContext.CancellationToken);
        var second = await runner.RunAsync(scripts, _target, TestContext.CancellationToken);
        TestContext.WriteLine(string.Join(",", executor.Names));

        // Assert
        CollectionAssert.AreEqual(new[] { "001_ext.sql", "020_queues.sql", "001_ext.sql", "020_queues.sql" }, executor.Names);
        Assert.HasCount(2, second.Ran);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RunAsync_OnceScript_RunsOnlyUntilItsHashIsRecorded()
    {
        var executor = new Recorder();
        var runner = new EnsureScriptRunner(executor, new InMemoryAppliedScriptStore());
        var v1 = new DesiredSchema([new DesiredScript("010_seed.once.sql", "INSERT 1")]);
        var v2 = new DesiredSchema([new DesiredScript("010_seed.once.sql", "INSERT 2")]);

        await runner.RunAsync(v1, _target, TestContext.CancellationToken);
        var again = await runner.RunAsync(v1, _target, TestContext.CancellationToken);
        var changed = await runner.RunAsync(v2, _target, TestContext.CancellationToken);

        Assert.AreEqual("010_seed.once.sql", again.Skipped[0]);
        Assert.IsEmpty(again.Ran);
        Assert.HasCount(1, changed.Ran);
        Assert.HasCount(2, executor.Names);
    }

    private sealed class Recorder : IScriptExecutor
    {
        public List<string> Names { get; } = [];

        public Task ExecuteAsync(DatabaseTarget target, string scriptName, string scriptText, CancellationToken cancellationToken = default)
        {
            Names.Add(scriptName);
            return Task.CompletedTask;
        }
    }
}
