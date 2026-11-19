using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System.Buffers;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Tests;

/// <summary>Out-of-process plugins found by <see cref="PluginLoader"/> run only after approval (<see cref="PluginTrust"/>).</summary>
[TestCategory("Unit")]
[DoNotParallelize]
[TestClass]
public sealed class PluginTrustTests
{
    private TempDirectoryHandle _homeFolder = null!;
    private string _home = "";
    private string _plugins = "";
    private string? _previousHome;

    [TestInitialize]
    public void Init()
    {
        _previousHome = Environment.GetEnvironmentVariable(DevTermHome.EnvironmentVariable);
        _homeFolder = new TempDirectoryHandle();
        _home = _homeFolder.DirectoryPath;
        _plugins = Path.Combine(_home, "plugins");
        Directory.CreateDirectory(_plugins);
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, _home);
        PluginTrust.Approver = null;
        LivePlugins.Reset();
    }

    [TestCleanup]
    public void Cleanup()
    {
        PluginTrust.Approver = null;
        LivePlugins.Reset();
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, _previousHome);
        _homeFolder.Dispose();
    }

    private string WriteProcessPlugin(string script = "print('x')", string command = "python")
    {
        var folder = Path.Combine(_plugins, "shout");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "shout.py"), script);
        File.WriteAllText(
            Path.Combine(folder, PluginManifest.FileName),
            $$"""{ "name": "py-shout", "version": "1.2.0", "contract": 1, "process": { "command": "{{command}}", "arguments": ["{folder}/shout.py"] } }""");
        return folder;
    }

    private PluginLoadResult Load(out ServiceCollection services)
    {
        services = [];
        return PluginLoader.LoadAll(_plugins, services).Single();
    }

    private sealed class Approver(PluginApprovalChoice choice) : IPluginApprover
    {
        public List<PluginApprovalRequest> Asked { get; } = [];

        public PluginApprovalChoice Ask(PluginApprovalRequest request)
        {
            Asked.Add(request);
            return choice;
        }
    }

    [TestMethod]
    public void WithNoApprover_AProcessPluginDoesNotLoad_AndSaysWhy()
    {
        WriteProcessPlugin();

        var result = Load(out var services);

        Assert.IsFalse(result.Loaded);
        Assert.Contains("needs your approval", result.Message);
        Assert.Contains("shout.py", result.Message);
        Assert.IsEmpty(services);
    }

    [TestMethod]
    public void AnUnapprovedPlugin_Waits_AndApprovingItMakesItsPresenterResolvableAtOnce()
    {
        WriteProcessPlugin();
        Load(out _);
        var catalog = new PresenterCatalog([]);
        Assert.IsFalse(catalog.TryGet("py-shout", out _));
        Assert.AreEqual("py-shout", LivePlugins.Pending.Single().Request.Name);

        Assert.IsTrue(LivePlugins.Approve("py-shout", PluginApprovalChoice.Once));

        Assert.IsEmpty(LivePlugins.Pending);
        Assert.IsTrue(catalog.TryGet("py-shout", out var presenter));
        Assert.AreEqual("py-shout", presenter.Name);
        CollectionAssert.Contains(catalog.Names.ToList(), "py-shout");
        Assert.IsFalse(PluginTrust.IsApproved("py-shout", PluginHash.Compute(Path.Combine(_plugins, "shout"))), "Once is not remembered.");
    }

    [TestMethod]
    public void ApprovingAlways_RemembersIt_AndDenyChangesNothing()
    {
        WriteProcessPlugin();
        Load(out _);

        Assert.IsFalse(LivePlugins.Approve("py-shout", PluginApprovalChoice.Deny));
        Assert.HasCount(1, LivePlugins.Pending);
        Assert.IsTrue(LivePlugins.Approve("py-shout", PluginApprovalChoice.Always));

        Assert.IsTrue(PluginTrust.IsApproved("py-shout", PluginHash.Compute(Path.Combine(_plugins, "shout"))));
        Assert.IsFalse(LivePlugins.Approve("nope", PluginApprovalChoice.Once));
    }

    [TestMethod]
    public void ReviewPending_AsksAboutEachWaitingPlugin_AndApprovesOnlyTheOnesAnsweredYes()
    {
        WriteProcessPlugin();
        Load(out _);
        var asked = new List<string>();

        var live = LivePlugins.ReviewPending(p =>
        {
            asked.Add(p.Request.Name);
            return PluginApprovalChoice.Once;
        });

        Assert.AreEqual(1, live);
        CollectionAssert.AreEqual(new[] { "py-shout" }, asked);
        Assert.AreEqual(0, LivePlugins.ReviewPending(_ => PluginApprovalChoice.Once), "Nothing is left to ask about.");
    }

    [TestMethod]
    public void Review_ForgetsTheApprovalsSaidForget_KeepsTheRest_AndStopsOnStop()
    {
        PluginTrust.Remember("a", "1");
        PluginTrust.Remember("b", "2");
        PluginTrust.Remember("c", "3");

        var forgotten = PluginTrust.Review(approval => approval.Name switch
        {
            "a" => ApprovalReviewChoice.Forget,
            "b" => ApprovalReviewChoice.Keep,
            _ => ApprovalReviewChoice.Stop,
        });

        Assert.AreEqual(1, forgotten);
        CollectionAssert.AreEqual(new[] { "b", "c" }, PluginTrust.Approvals().Select(a => a.Name).ToArray());
    }

    [TestMethod]
    public void Deny_DoesNotLoad_AndIsNotRemembered()
    {
        WriteProcessPlugin();
        var approver = new Approver(PluginApprovalChoice.Deny);
        PluginTrust.Approver = approver;

        Assert.IsFalse(Load(out _).Loaded);
        Assert.IsFalse(Load(out _).Loaded);

        Assert.HasCount(2, approver.Asked);
    }

    [TestMethod]
    public void Once_Loads_ButAsksAgainNextTime()
    {
        WriteProcessPlugin();
        var approver = new Approver(PluginApprovalChoice.Once);
        PluginTrust.Approver = approver;

        Assert.IsTrue(Load(out var services).Loaded);
        Assert.IsTrue(Load(out _).Loaded);

        Assert.HasCount(2, approver.Asked);
        Assert.AreEqual("py-shout", services.BuildServiceProvider().GetServices<IPresenter>().Single().Name);
        Assert.AreEqual("1.2.0", approver.Asked[0].Version);
        Assert.Contains("shout.py", approver.Asked[0].CommandLine);
    }

    [TestMethod]
    public void Always_IsRemembered_SoAnUnchangedPluginIsNotAskedAgain_ButAnEditedOneIs()
    {
        var folder = WriteProcessPlugin();
        var approver = new Approver(PluginApprovalChoice.Always);
        PluginTrust.Approver = approver;

        Assert.IsTrue(Load(out _).Loaded);
        Assert.HasCount(1, approver.Asked);
        Assert.IsTrue(File.Exists(PluginTrust.StorePath));

        // Unchanged, and even with nobody to ask: runs on the remembered approval.
        PluginTrust.Approver = null;
        Assert.IsTrue(Load(out _).Loaded);

        // A changed script is different content, so the old approval no longer applies.
        File.WriteAllText(Path.Combine(folder, "shout.py"), "print('changed')");
        Assert.IsFalse(Load(out _).Loaded);

        PluginTrust.Approver = approver;
        Assert.IsTrue(Load(out _).Loaded);
        Assert.HasCount(2, approver.Asked);
        Assert.AreNotEqual(approver.Asked[0].Hash, approver.Asked[1].Hash);
    }

    [TestMethod]
    public void Forget_RemovesARememberedApproval()
    {
        WriteProcessPlugin();
        PluginTrust.Approver = new Approver(PluginApprovalChoice.Always);
        Assert.IsTrue(Load(out _).Loaded);

        PluginTrust.Forget("py-shout");
        PluginTrust.Approver = null;

        Assert.IsFalse(Load(out _).Loaded);
    }

    [TestMethod]
    public void ACorruptApprovalsFile_IsTreatedAsNothingApproved()
    {
        WriteProcessPlugin();
        File.WriteAllText(PluginTrust.StorePath, "{ not json");

        Assert.IsFalse(Load(out _).Loaded);
    }

    [TestMethod]
    public void AManifestWithNeitherAnAssemblyNorACommand_IsRejected()
    {
        var folder = Path.Combine(_plugins, "empty");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, PluginManifest.FileName), """{ "name": "x", "contract": 1 }""");

        var result = Load(out _);

        Assert.IsFalse(result.Loaded);
        Assert.Contains("process command", result.Message);
    }

    [TestMethod]
    public async Task TheLazyPresenter_RenderReturnsAtOnce_AndTheSessionStillGetsTheLinesWhenThePluginAnswers()
    {
        ToolProbe.RequireOrSkip("python", "--version");

        var script = Path.Combine(_home, "slow.py");
        await File.WriteAllTextAsync(script, """
            import sys, json, time
            for line in sys.stdin:
                m = json.loads(line)
                if m["type"] == "hello":
                    print(json.dumps({"type": "hello", "name": "slow", "protocol": 1}), flush=True)
                else:
                    time.sleep(0.5)
                    print(json.dumps({"type": "output", "lines": ["decoded"]}), flush=True)
            """);
        await AssertLazyAsync(new LazyExternalPresenter("slow", "python", [script], TimeSpan.FromSeconds(10)));
    }

    [TestMethod]
    public async Task TheLazyPresenter_JavaPlugin_RenderReturnsAtOnce_AndTheSessionStillGetsTheLines()
    {
        ToolProbe.RequireOrSkip("java", "-version");

        var script = Path.Combine(_home, "Slow.java");
        await File.WriteAllTextAsync(script, """
            import java.io.*;
            public class Slow {
                public static void main(String[] args) throws Exception {
                    var in = new BufferedReader(new InputStreamReader(System.in));
                    String line;
                    while ((line = in.readLine()) != null) {
                        if (line.contains("\"hello\"")) {
                            System.out.println("{\"type\":\"hello\",\"name\":\"slow\",\"protocol\":1}");
                        } else {
                            Thread.sleep(500);
                            System.out.println("{\"type\":\"output\",\"lines\":[\"decoded\"]}");
                        }
                        System.out.flush();
                    }
                }
            }
            """);
        await AssertLazyAsync(new LazyExternalPresenter("slow", "java", [script], TimeSpan.FromSeconds(30)));
    }

    private static async Task AssertLazyAsync(LazyExternalPresenter presenter)
    {
        await using var _ = presenter;
        var pipeline = new Pipeline([presenter]);
        var seen = new List<PresenterOutput>();
        pipeline.AsyncOutput += (_, o) => seen.Add(o);

        var started = global::System.Diagnostics.Stopwatch.StartNew();
        var immediate = pipeline.Render(new ReadOnlySequence<byte>("abc"u8.ToArray()));
        var renderMs = started.ElapsedMilliseconds;
        await presenter.FlushAsync();

        Assert.IsEmpty(immediate);
        Assert.IsLessThan(400, renderMs);
        Assert.HasCount(1, seen);
        Assert.AreEqual("decoded", seen[0].Text);
        Assert.AreEqual("slow", seen[0].PresenterName);
    }

    [TestMethod]
    public async Task TheLazyPresenter_DoesNotStartAProgramUntilItRenders_AndAProgramThatWontStartRendersNothing()
    {
        var presenter = new LazyExternalPresenter("gone", Path.Combine(_home, "no-such-program.exe"), [], TimeSpan.FromSeconds(1));

        Assert.IsFalse(presenter.Faulted);
        Assert.IsEmpty(presenter.Render(new ReadOnlySequence<byte>("abc"u8.ToArray())));
        await presenter.FlushAsync();
        Assert.IsTrue(presenter.Faulted);
        Assert.Contains("no-such-program", presenter.FaultReason!);
    }
}

