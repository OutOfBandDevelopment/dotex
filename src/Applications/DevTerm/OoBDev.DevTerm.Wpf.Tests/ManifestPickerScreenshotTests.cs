using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.DevTerm.Wpf;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The manifest picker window listing a bundled and a user manifest, captured for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class ManifestPickerScreenshotTests
{
    [TestMethod]
    public void ManifestPickerWindow_TwoEntries_IsCaptured() =>
        WpfScreenshot.Capture(
            () => new ManifestPickerWindow(new ManifestPickerDialogViewModel(
            [
                new ManifestEntry("Busylight", "busylight.json", "bundled"),
                new ManifestEntry("Bench supply", "bench-supply.json", "user"),
            ])),
            "wpf-manifest-picker.png",
            460,
            320);
}
