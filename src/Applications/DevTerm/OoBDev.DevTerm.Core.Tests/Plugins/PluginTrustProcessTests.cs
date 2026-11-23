using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System.Buffers;
using System.Text;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Tests;

/// <summary>An approved plugin found in a plugins folder runs end to end (Python, Java and Go examples; each is Inconclusive without its runtime).</summary>
[TestCategory("Unit")]
[DoNotParallelize]
[TestClass]
public sealed class PluginTrustProcessTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public Task AnApprovedPythonPlugin_FoundByTheLoader_RendersThroughTheCatalog() =>
        RunApprovedExampleAsync("python", "python", "--version", "shout.py", "py-shout", ["{folder}/shout.py"], 10000, "HELLO");

    [TestMethod]
    public Task AnApprovedJavaPlugin_FoundByTheLoader_RendersThroughTheCatalog() =>
        RunApprovedExampleAsync("java", "java", "-version", "Shout.java", "java-count", ["{folder}/Shout.java"], 30000, "5 bytes");

    [TestMethod]
    public Task AnApprovedGoPlugin_FoundByTheLoader_RendersThroughTheCatalog() =>
        RunApprovedExampleAsync("go", "go", "version", "main.go", "go-reverse", ["run", "{folder}/main.go"], 120000, "olleh");

    private static async Task RunApprovedExampleAsync(string language, string command, string probeArgument, string file, string name, string[] arguments, int replyTimeoutMs, string expected)
    {
        ToolProbe.RequireOrSkip(command, probeArgument);
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "GitVersion.yml")))
        {
            root = root.Parent;
        }

        using var homeFolder = new TempDirectoryHandle();
        var home = homeFolder.DirectoryPath;
        var previous = Environment.GetEnvironmentVariable(DevTermHome.EnvironmentVariable);
        var folder = Path.Combine(home, "plugins", "shout");
        Directory.CreateDirectory(folder);
        try
        {
            Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, home);
            File.Copy(Path.Combine(root!.FullName, "examples", "devterm", language, "out-of-process-plugin", file), Path.Combine(folder, file));
            File.WriteAllText(
                Path.Combine(folder, PluginManifest.FileName),
                global::System.Text.Json.JsonSerializer.Serialize(new { name, version = "1.0.0", contract = 1, process = new { command, arguments, replyTimeoutMs } }));
            PluginTrust.Approver = new AlwaysApprover();

            var services = new ServiceCollection();
            Assert.IsTrue(PluginLoader.LoadAll(Path.Combine(home, "plugins"), services).Single().Loaded);
            await using var provider = services.BuildServiceProvider();
            var presenter = new PresenterCatalog(provider.GetServices<IPresenter>()).Get(name);

            var lazy = (LazyExternalPresenter)presenter;
            var lines = new List<string>();
            lazy.OutputReady += (_, l) => lines.AddRange(l);
            Assert.IsEmpty(presenter.Render(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("hello"))));
            await lazy.FlushAsync();

            CollectionAssert.AreEqual(new[] { expected }, lines);
        }
        finally
        {
            PluginTrust.Approver = null;
            Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, previous);
        }
    }

    [TestMethod]
    public async Task AnApprovedTransportPlugin_FoundByTheLoader_IsChosenByTheTransportOption()
    {
        ToolProbe.RequireOrSkip("pwsh", "-NoProfile -Command exit");
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "GitVersion.yml")))
        {
            root = root.Parent;
        }

        using var homeFolder = new TempDirectoryHandle();
        var home = homeFolder.DirectoryPath;
        var previous = Environment.GetEnvironmentVariable(DevTermHome.EnvironmentVariable);
        var folder = Path.Combine(home, "plugins", "ps-echo");
        Directory.CreateDirectory(folder);
        try
        {
            Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, home);
            var example = Path.Combine(root!.FullName, "examples", "devterm", "powershell", "out-of-process-transport");
            foreach (var file in new[] { "echo.ps1", PluginManifest.FileName })
            {
                File.Copy(Path.Combine(example, file), Path.Combine(folder, file));
            }

            PluginTrust.Approver = new AlwaysApprover();
            var services = new ServiceCollection();
            OoBDev.DevTerm.Configuration.ServiceCollectionExtensions.AddPlugins(services, new OoBDev.DevTerm.Configuration.CliOptions { Transport = "PS-Echo", Plugins = Path.Combine(home, "plugins") });
            await using var provider = services.BuildServiceProvider();

            await using var adapter = provider.GetRequiredService<IDeviceAdapter>();
            await adapter.OpenAsync(TestContext.CancellationToken);
            await adapter.Output.WriteAsync(Encoding.ASCII.GetBytes("hi"), TestContext.CancellationToken);
            var read = await adapter.Input.ReadAsync(TestContext.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(15), TestContext.CancellationToken);

            Assert.AreEqual("hi", Encoding.ASCII.GetString(read.Buffer.ToArray()));
            Assert.AreEqual("plugin://ps-echo", adapter.Path);
            Assert.AreEqual(0, provider.GetServices<IPresenter>().Count());
        }
        finally
        {
            PluginTrust.Approver = null;
            Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, previous);
        }
    }

    [TestMethod]
    public void Approvals_ListsWhatWasRemembered_AndForgetRemovesIt()
    {
        using var homeFolder = new TempDirectoryHandle();
        var home = homeFolder.DirectoryPath;
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
        }
    }

    private sealed class AlwaysApprover : IPluginApprover
    {
        public PluginApprovalChoice Ask(PluginApprovalRequest request) => PluginApprovalChoice.Once;
    }
}
