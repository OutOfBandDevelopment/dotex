using System.IO;
using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The theme seed picker and theme builder windows, captured for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class ThemeWindowsScreenshotTests
{
    private static string Directory_() => Directory.CreateTempSubdirectory().FullName;

    [TestMethod]
    public void ThemeSeedPickerWindow_BuiltInThemes_IsCaptured() =>
        WpfScreenshot.Capture(
            () => new ThemeSeedPickerWindow(new ThemeSeedPickerViewModel(null, Directory_())),
            "wpf-theme-seed-picker.png",
            460,
            360);

    [TestMethod]
    public void ThemeBuilderWindow_DarkSeed_IsCaptured()
    {
        var seed = ActiveTheme.Catalog.Resolve(BuiltInThemes.DarkName, out _);
        WpfScreenshot.Capture(
            () => new ThemeBuilderWindow(new ThemeBuilderDialogViewModel(new ThemeBuilderState(seed, "Mine"), null, null, Directory_())),
            "wpf-theme-builder.png",
            620,
            480);
    }
}
