using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.DeviceManifests;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The device manifest picker opened from the shell.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ManifestPickerDialogTests
{
    private sealed class FakeManifestPickerDialog(Func<ManifestPickerDialogViewModel, bool> interact) : IManifestPickerDialog
    {
        public bool Pick(ManifestPickerDialogViewModel picker) => interact(picker);
    }

    private static string WriteManifest(string directory)
    {
        var manifest = new DeviceManifest
        {
            Name = "Test Supply",
            Terminator = "\n",
            OutboundCommands = [new OutboundCommand { Id = "read", Name = "Read", Template = "READ?", IsQuery = true }],
        };
        var path = Path.Combine(directory, "device.json");
        File.WriteAllText(path, DeviceManifestWriter.ToFileJson(manifest));
        return path;
    }

    [TestMethod]
    public void TryLoad_TypedPathOfAManifest_LoadsIt()
    {
        // Stage
        var directory = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var picker = new ManifestPickerDialogViewModel([])
            {
                PathText = $"\"{WriteManifest(directory)}\"",
            };

            // Test
            var loaded = picker.TryLoad();

            // Assert
            Assert.IsTrue(loaded);
            Assert.AreEqual("Test Supply", picker.Chosen!.Name);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    public void TryLoad_NothingPicked_ReportsAnError()
    {
        // Stage
        var picker = new ManifestPickerDialogViewModel([]);

        // Test
        var loaded = picker.TryLoad();

        // Assert
        Assert.IsFalse(loaded);
        Assert.AreNotEqual(string.Empty, picker.Error);
    }

    [TestMethod]
    public void TryLoad_MissingPath_ReportsAnErrorAndKeepsPickerOpen()
    {
        // Stage
        var picker = new ManifestPickerDialogViewModel([]) { PathText = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.json") };

        // Test
        var loaded = picker.TryLoad();

        // Assert
        Assert.IsFalse(loaded);
        StringAssert.Contains(picker.Error, "Could not load");
    }

    [TestMethod]
    public void TryLoad_ListedEntry_LoadsTheSelectedEntry()
    {
        // Stage
        var directory = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var picker = new ManifestPickerDialogViewModel([new ManifestEntry("Test Supply", WriteManifest(directory), "user")]);

            // Test
            var loaded = picker.TryLoad();

            // Assert
            Assert.IsTrue(loaded);
            Assert.AreEqual(0, picker.SelectedIndex);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    public void OpenManifestPanel_ChosenManifest_OpensItsPanel()
    {
        // Stage
        var directory = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var path = WriteManifest(directory);
            var services = new ServiceCollection();
            services.AddSingleton<IManifestPickerDialog>(new FakeManifestPickerDialog(picker =>
            {
                picker.PathText = path;
                return picker.TryLoad();
            }));
            using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();
            shell.NewLoopbackSession.Execute(null);

            // Test
            shell.OpenManifestPanel.Execute(null);

            // Assert
            Assert.AreEqual("Test Supply", shell.SelectedPanel!.Title);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    public void OpenManifestPanel_NoSession_CannotExecute()
    {
        // Stage
        using var shell = new ShellViewModel([], manifestPickerDialog: new FakeManifestPickerDialog(_ => false));

        // Assert
        Assert.IsFalse(shell.OpenManifestPanel.CanExecute(null));
    }
}
