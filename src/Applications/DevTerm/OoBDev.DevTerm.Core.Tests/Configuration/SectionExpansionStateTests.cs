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

/// <summary>
/// <see cref="SectionExpansionState"/> — the remembered expand/collapse state both control-panel
/// renderers restore when a panel reopens — and <see cref="InstalledManifests"/>, which finds the
/// manifests bundled next to the app.
/// </summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class SectionExpansionStateTests
{
    [TestMethod]
    public void UnknownSection_IsExpanded_AndASetStateIsKeyedByDefinitionAndSection()
    {
        const string definition = "SectionExpansionStateTests.Panel";
        SectionExpansionState.Forget(definition);
        try
        {
            Assert.IsTrue(SectionExpansionState.IsExpanded(definition, "Measure"));

            SectionExpansionState.Set(definition, "Measure", expanded: false);

            Assert.IsFalse(SectionExpansionState.IsExpanded(definition, "Measure"));
            Assert.IsTrue(SectionExpansionState.IsExpanded(definition, "Source"), "Another section of the same panel is unaffected.");
            Assert.IsTrue(SectionExpansionState.IsExpanded(definition + ".Other", "Measure"), "The same section label on another panel is unaffected.");

            SectionExpansionState.Set(definition, "Measure", expanded: true);
            Assert.IsTrue(SectionExpansionState.IsExpanded(definition, "Measure"));
        }
        finally
        {
            SectionExpansionState.Forget(definition);
        }
    }

    [TestMethod]
    public void UsePersistence_SavesCollapsedSections_AndRestoresThemFromTheFile()
    {
        const string definition = "SectionExpansionStateTests.Persisted";
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, "section-state.json");
        try
        {
            SectionExpansionState.UsePersistence(path);
            SectionExpansionState.Set(definition, "Measure", expanded: false);
            SectionExpansionState.Set(definition, "Source", expanded: true);
            SectionExpansionState.StopPersistence();

            SectionExpansionState.Forget(definition);
            Assert.IsTrue(SectionExpansionState.IsExpanded(definition, "Measure"), "Forgotten in memory, as after a restart.");

            SectionExpansionState.UsePersistence(path);
            Assert.IsFalse(SectionExpansionState.IsExpanded(definition, "Measure"), "Loaded back from the file.");
            Assert.IsTrue(SectionExpansionState.IsExpanded(definition, "Source"));
        }
        finally
        {
            SectionExpansionState.StopPersistence();
            SectionExpansionState.Forget(definition);
        }
    }

    [TestMethod]
    public void Forget_RestoresTheDefaultForOnlyThatDefinition()
    {
        SectionExpansionState.Set("A.Forget", "S", expanded: false);
        SectionExpansionState.Set("B.Forget", "S", expanded: false);

        SectionExpansionState.Forget("A.Forget");

        Assert.IsTrue(SectionExpansionState.IsExpanded("A.Forget", "S"));
        Assert.IsFalse(SectionExpansionState.IsExpanded("B.Forget", "S"));
        SectionExpansionState.Forget("B.Forget");
    }

    [TestMethod]
    public void InstalledManifests_IncludesTheBundledLoopbackDemo()
    {
        var entry = InstalledManifests.Discover().FirstOrDefault(e => e.Name == "Loopback Sensor Demo" && e.Source == InstalledManifests.InstalledSource);

        Assert.IsNotNull(entry, "The bundled manifest is copied to manifests\\ next to every app referencing DevTerm.Configuration.");
        Assert.AreEqual(InstalledManifests.InstalledSource, entry.Source);
    }
}
