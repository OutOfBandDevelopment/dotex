using System;
using System.Collections.Generic;
using System.IO;
using OoBDev.System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;

using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Configuration.Discovery;
namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class ManifestPanelHintTests
{
    [TestMethod]
    public void For_NoManifestName_ReturnsNull() => Assert.IsNull(ManifestPanelHint.For(new CliOptions()));

    [TestMethod]
    public void For_ManifestThatDoesNotResolve_ReturnsNull_TheWarningsJob() =>
        Assert.IsNull(ManifestPanelHint.For(new CliOptions { ManifestName = "panel-hint-test-" + Guid.NewGuid().ToString("N") }));

    [TestMethod]
    public void For_TheBundledDemoManifest_OffersItsPanel()
    {
        var hint = ManifestPanelHint.For(new CliOptions { ManifestName = "loopback-sensor-demo" }, TempStore());

        Assert.IsNotNull(hint);
        Assert.Contains("loopback-sensor-demo", hint);
        Assert.Contains("Device Manifest", hint);
    }

    [TestMethod]
    public void MarkUsed_StopsTheHint_AndSurvivesAReload_ForThatManifestOnly()
    {
        var store = TempStore();
        var options = new CliOptions { ManifestName = "loopback-sensor-demo" };
        Assert.IsNotNull(ManifestPanelHint.For(options, store));

        ManifestPanelHint.MarkUsed("Loopback-Sensor-Demo", store);
        ManifestPanelHint.MarkUsed("loopback-sensor-demo", store);

        Assert.IsNull(ManifestPanelHint.For(options, new AppPreferencesStore(store.Path)));
        Assert.HasCount(1, store.Load().UsedPanelHints);
    }

    private readonly List<TempDirectoryHandle> _directories = [];

    [TestCleanup]
    public void DeleteTempDirectories()
    {
        foreach (var handle in _directories)
        {
            handle.Dispose();
        }
    }

    private string CreateTempDirectory()
    {
        var handle = new TempDirectoryHandle();
        _directories.Add(handle);
        return handle.DirectoryPath;
    }

    private AppPreferencesStore TempStore() =>
        new(Path.Combine(CreateTempDirectory(), "preferences.json"));
}
