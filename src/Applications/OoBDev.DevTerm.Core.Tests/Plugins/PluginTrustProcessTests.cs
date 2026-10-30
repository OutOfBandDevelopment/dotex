using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System.Buffers;
using global::System.Diagnostics;
using System.Text;

namespace OoBDev.DevTerm.Tests;

/// <summary>An approved plugin found in a plugins folder runs end to end (needs python; Inconclusive without it).</summary>
[TestCategory("Unit")]
[DoNotParallelize]
[TestClass]
public sealed class PluginTrustProcessTests
{
    [TestMethod]
    public async Task AnApprovedPythonPlugin_FoundByTheLoader_RendersThroughTheCatalog()
    {
        PythonProbe.RequireOrSkip();


        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "DevTerm.slnx")))
        {
            root = root.Parent;
        }

        var home = Path.Combine(Path.GetTempPath(), "devterm-trust-" + Guid.NewGuid().ToString("N"));
        var previous = Environment.GetEnvironmentVariable(DevTermHome.EnvironmentVariable);
        var folder = Path.Combine(home, "plugins", "shout");
        Directory.CreateDirectory(folder);
        try
        {
            Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, home);
            File.Copy(Path.Combine(root!.FullName, "examples", "python", "out-of-process-plugin", "shout.py"), Path.Combine(folder, "shout.py"));
            File.WriteAllText(
                Path.Combine(folder, PluginManifest.FileName),
                """{ "name": "py-shout", "version": "1.0.0", "contract": 1, "process": { "command": "python", "arguments": ["{folder}/shout.py"], "replyTimeoutMs": 10000 } }""");
            PluginTrust.Approver = new AlwaysApprover();

            var services = new ServiceCollection();
            Assert.IsTrue(PluginLoader.LoadAll(Path.Combine(home, "plugins"), services).Single().Loaded);
            await using var provider = services.BuildServiceProvider();
            var presenter = new PresenterCatalog(provider.GetServices<IPresenter>()).Get("py-shout");

            var lazy = (LazyExternalPresenter)presenter;
            var lines = new List<string>();
            lazy.OutputReady += (_, l) => lines.AddRange(l);
            Assert.IsEmpty(presenter.Render(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("hello"))));
            await lazy.FlushAsync();

            CollectionAssert.AreEqual(new[] { "HELLO" }, lines);
        }
        finally
        {
            PluginTrust.Approver = null;
            Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, previous);
            try
            {
                Directory.Delete(home, true);
            }
            catch (IOException)
            {
            }
        }
    }

    [TestMethod]
    public void Approvals_ListsWhatWasRemembered_AndForgetRemovesIt()
    {
        var home = Path.Combine(Path.GetTempPath(), "devterm-trust-" + Guid.NewGuid().ToString("N"));
        var previous = Environment.GetEnvironmentVariable(DevTermHome.EnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, home);
            Assert.AreEqual(0, PluginTrust.Approvals().Count);

            PluginTrust.Remember("shout", "AB12");
            PluginTrust.Remember("whisper", "CD34");

            CollectionAssert.AreEqual(new[] { "shout", "whisper" }, PluginTrust.Approvals().Select(a => a.Name).ToArray());
            PluginTrust.Forget("shout");
            CollectionAssert.AreEqual(new[] { "whisper" }, PluginTrust.Approvals().Select(a => a.Name).ToArray());
        }
        finally
        {
            Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, previous);
            try
            {
                Directory.Delete(home, true);
            }
            catch (IOException)
            {
            }
        }
    }

    private sealed class AlwaysApprover : IPluginApprover
    {
        public PluginApprovalChoice Ask(PluginApprovalRequest request) => PluginApprovalChoice.Once;
    }
}
