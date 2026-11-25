using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.DevTerm.Hosting;
using System.Buffers;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class PluginLoaderTests
{
    private static string SamplePluginFolder => Path.Combine(AppContext.BaseDirectory, "plugins", "sample");

    private static void CopySample(string pluginsDirectory, string folder, Func<string, string>? manifestEdit = null)
    {
        var target = Path.Combine(pluginsDirectory, folder);
        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(SamplePluginFolder))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        }

        if (manifestEdit is not null)
        {
            var manifest = Path.Combine(target, PluginManifest.FileName);
            File.WriteAllText(manifest, manifestEdit(File.ReadAllText(manifest)));
        }
    }

    [TestMethod]
    public void LoadAll_RegistersAPluginsPresenter_SoTheCatalogFindsItAndItRenders()
    {
        var services = new ServiceCollection().AddDevTermCore();

        var results = PluginLoader.LoadAll(Path.GetDirectoryName(SamplePluginFolder), services);

        Assert.AreEqual(1, results.Count, string.Join("; ", results.Select(r => r.Message)));
        Assert.IsTrue(results[0].Loaded, results[0].Message);
        using var provider = services.BuildServiceProvider();
        var presenter = provider.GetRequiredService<PresenterCatalog>().Get("sample");
        Assert.AreEqual("sample: 3 bytes", presenter.Render(new ReadOnlySequence<byte>([1, 2, 3]))[0]);
        // The plugin's presenter implements the host's own IPresenter, not a second copy loaded beside it.
        Assert.AreSame(typeof(IPresenter).Assembly, presenter.GetType().GetInterface(nameof(IPresenter))!.Assembly);
    }

    [TestMethod]
    public void LoadAll_LowerOrderFirst_ThenFolderName()
    {
        using var pluginsFolder = new TempDirectoryHandle();
        var directory = pluginsFolder.DirectoryPath;
        CopySample(directory, "a-late", text => text.Replace("\"contract\": 1", "\"contract\": 1, \"order\": 5", StringComparison.Ordinal));
        CopySample(directory, "c-plain");
        CopySample(directory, "b-plain");
        CopySample(directory, "z-early", text => text.Replace("\"contract\": 1", "\"contract\": 1, \"order\": -1", StringComparison.Ordinal));

        var results = PluginLoader.LoadAll(directory, new ServiceCollection());

        var order = results.Select(r => Path.GetFileName(r.Folder)).ToList();
        Assert.AreEqual("z-early,b-plain,c-plain,a-late", string.Join(",", order));
    }

    [TestMethod]
    public void LoadAll_SkipsAPluginBuiltForAnotherContract_AndSaysWhy()
    {
        using var pluginsFolder = new TempDirectoryHandle();
        var directory = pluginsFolder.DirectoryPath;
        CopySample(directory, "future", text => text.Replace("\"contract\": 1", "\"contract\": 99", StringComparison.Ordinal));
        var services = new ServiceCollection();

        var results = PluginLoader.LoadAll(directory, services);

        Assert.IsFalse(results.Single().Loaded);
        StringAssert.Contains(results.Single().Message, "contract 99");
        Assert.AreEqual(0, services.Count);
    }

    [TestMethod]
    public void LoadAll_ABrokenPluginDoesNotStopTheGoodOnes()
    {
        using var pluginsFolder = new TempDirectoryHandle();
        var directory = pluginsFolder.DirectoryPath;
        Directory.CreateDirectory(Path.Combine(directory, "a-broken"));
        File.WriteAllText(Path.Combine(directory, "a-broken", PluginManifest.FileName), "{ not json");
        CopySample(directory, "b-good");
        Directory.CreateDirectory(Path.Combine(directory, "no-manifest"));
        var services = new ServiceCollection();

        var results = PluginLoader.LoadAll(directory, services);

        Assert.AreEqual(2, results.Count);
        Assert.IsFalse(results[0].Loaded);
        Assert.IsTrue(results[1].Loaded, results[1].Message);
    }

    [TestMethod]
    public void LoadAll_RefusesAnAssemblyPathOutsideThePluginFolder()
    {
        using var pluginsFolder = new TempDirectoryHandle();
        var directory = pluginsFolder.DirectoryPath;
        CopySample(directory, "escape", text => text.Replace("OoBDev.DevTerm.Core.TestPlugin.dll", "..\\\\outside.dll", StringComparison.Ordinal));

        var result = PluginLoader.LoadAll(directory, new ServiceCollection()).Single();

        Assert.IsFalse(result.Loaded);
        StringAssert.Contains(result.Message, "isn't in the plugin folder");
    }

    [TestMethod]
    public void LoadAll_AMissingOrEmptyDirectoryLoadsNothing()
    {
        var services = new ServiceCollection();

        Assert.AreEqual(0, PluginLoader.LoadAll(null, services).Count);
        Assert.AreEqual(0, PluginLoader.LoadAll(Path.Combine(Path.GetTempPath(), "devterm-no-such-dir"), services).Count);
    }
}
