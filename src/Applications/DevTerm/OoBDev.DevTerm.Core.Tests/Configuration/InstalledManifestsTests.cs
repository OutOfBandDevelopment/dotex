using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Configuration.Tests;

/// <summary><see cref="InstalledManifests"/> finds the manifests bundled next to the app.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class InstalledManifestsTests
{
    [TestMethod]
    public void Discover_BundledLoopbackDemo_IsIncluded()
    {
        var entry = InstalledManifests.Discover().FirstOrDefault(e => e.Name == "Loopback Sensor Demo" && e.Source == InstalledManifests.InstalledSource);

        Assert.IsNotNull(entry, "The bundled manifest is copied to manifests folder next to every app referencing DevTerm.Configuration.");
        Assert.AreEqual(InstalledManifests.InstalledSource, entry.Source);
    }
}
