using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.TestUtilities;
using OoBDev.Documents.Guides;

namespace OoBDev.DevTerm.DeviceManifests.Tests;

/// <summary>Generates the binary-format guide into <c>docs/applications/devterm/api-guide/binary-formats.md</c> from the device <c>.ksy</c> files (design: docs/design/GeneratedGuides).</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class KsyGuideTests
{
    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitVersion.yml")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find GitVersion.yml.");
    }

    [TestMethod]
    public void Guide_FromEveryDeviceKsyFile_HasASectionAndFieldTablePerFormat()
    {
        // Stage
        var root = RepoRoot();
        var files = Directory.EnumerateFiles(Path.Combine(root, "src", "HardwareDevices"), "*.ksy", SearchOption.AllDirectories)
            .Where(f => f.Contains($"{Path.DirectorySeparatorChar}docs{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(f => (Path.GetFileName(f), File.ReadAllText(f)))
            .ToList();

        // Test
        var guide = KsyGuideRenderer.Render("Device binary formats", files);
        ApiGuideRenderer.WriteIfChanged(Path.Combine(root, "docs", "applications", "devterm", "api-guide", "binary-formats.md"), guide);

        // Assert
        Assert.IsGreaterThanOrEqualTo(10, files.Count);
        StringAssert.Contains(guide, "### busylight_command");
        StringAssert.Contains(guide, "| `red` | u1 | 1 |");
        Assert.AreEqual(files.Count, guide.Split("\n**Table ", StringSplitOptions.None).Length - 1, "one field table per format");
    }
}
