using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.DevTerm.DeviceManifests.Editing;
using OoBDev.DevTerm.TestUtilities;
using OoBDev.DevTerm.Wpf;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>
/// Layout regression checks for the WPF dialogs, in both built-in themes: each window is shown off-screen
/// and checked by <see cref="WpfLayoutAssert"/> (nothing off-window, overlapping, clipped, too small to use
/// or unreadable). Deliberate exceptions are <see cref="LayoutAllowance"/>s, each with its reason.
/// </summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class UiLayoutReviewTests
{
    [TestCleanup]
    public void ResetTheme() => ActiveTheme.Reset();

    private static void Review(string theme, Func<Window> build, double width, double height)
    {
        WpfScreenshot.RunOnSta(() =>
        {
            ActiveTheme.Select(theme, persist: false);
            var window = build();
            try
            {
                WpfScreenshot.ShowOffScreen(window, width, height);
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
                window.UpdateLayout();
                WpfLayoutAssert.Check(window, new LayoutCheckOptions { IsDarkTheme = theme == BuiltInThemes.DarkName }, window.GetType().Name + " (" + theme + ")");
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static string TempDirectory() => Directory.CreateTempSubdirectory().FullName;

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void ThemeSeedPickerWindow_BuiltInThemes_HasNoLayoutProblems(string theme) =>
        Review(theme, () => new ThemeSeedPickerWindow(new ThemeSeedPickerViewModel(null, TempDirectory())), 460, 360);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void ThemeBuilderWindow_Seeded_HasNoLayoutProblems(string theme) =>
        Review(
            theme,
            () => new ThemeBuilderWindow(new ThemeBuilderDialogViewModel(new ThemeBuilderState(ActiveTheme.Catalog.Resolve(theme, out _), "Mine"), null, null, TempDirectory())),
            620,
            480);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void ManifestPickerWindow_TwoEntries_HasNoLayoutProblems(string theme) =>
        Review(
            theme,
            () => new ManifestPickerWindow(new ManifestPickerDialogViewModel(
            [
                new ManifestEntry("Busylight", "busylight.json", "bundled"),
                new ManifestEntry("Bench supply", "bench-supply.json", "user"),
            ])),
            460,
            320);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void ConverterToolsWindow_Empty_HasNoLayoutProblems(string theme) =>
        Review(theme, () => new ConverterToolsWindow(new ConverterToolsDialogViewModel(new ConverterToolsEditor([]))), 700, 450);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void StreamMonitorWindow_Empty_HasNoLayoutProblems(string theme) =>
        Review(theme, () => new StreamMonitorWindow(new StreamMonitorDialogViewModel(new StreamMonitor())), 900, 650);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void MainWindow_Connected_HasNoLayoutProblems(string theme) =>
        Review(theme, ShellScreenshotTests.BuildWindow, 900, 650);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void RoutingWindow_WithTraffic_HasNoLayoutProblems(string theme) =>
        Review(theme, RoutingScreenshotTests.BuildWindow, 760, 760);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void ConfigureDeviceWindow_AfterEdit_HasNoLayoutProblems(string theme) =>
        Review(theme, ToolWindowScreenshotTests.BuildConfigureDevice, 640, 520);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName, "tcp")]
    [DataRow(BuiltInThemes.DarkName, "tcp")]
    [DataRow(BuiltInThemes.LightName, "serial")]
    [DataRow(BuiltInThemes.DarkName, "serial")]
    [DataRow(BuiltInThemes.LightName, "loopback")]
    [DataRow(BuiltInThemes.DarkName, "loopback")]
    public void ConnectionEditorWindow_EachTransport_HasNoLayoutProblems(string theme, string transport) =>
        Review(
            theme,
            () =>
            {
                var editor = new ConnectionEditorViewModel(new ConnectionProfileStore(TempDirectory()), new CliOptions { Transport = transport });
                return new ConnectionEditorWindow(editor, scans: new ConnectionDeviceScans(editor));
            },
            900,
            700);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName, "round({volts} * {amps}, 1)", PickerMode.Expression)]
    [DataRow(BuiltInThemes.DarkName, "round({volts} * {amps}, 1)", PickerMode.Expression)]
    [DataRow(BuiltInThemes.LightName, "round({volts} * ", PickerMode.Expression)]
    [DataRow(BuiltInThemes.DarkName, "round({volts} * ", PickerMode.Expression)]
    [DataRow(BuiltInThemes.LightName, "round({volts} * 100, 0); {amps}", PickerMode.ExpressionList)]
    [DataRow(BuiltInThemes.DarkName, "round({volts} * 100, 0); {amps}", PickerMode.ExpressionList)]
    [DataRow(BuiltInThemes.LightName, "volts:Volts:#FF6600; amps", PickerMode.Channels)]
    [DataRow(BuiltInThemes.DarkName, "volts:Volts:#FF6600; amps", PickerMode.Channels)]
    public void ExpressionPickerWindow_Mode_HasNoLayoutProblems(string theme, string text, PickerMode mode) =>
        Review(theme, () => new ExpressionPickerWindow(new ExpressionPickerViewModel(ExpressionPickerScreenshotTests.Paths, text, seed: 7, mode: mode)), 560, 460);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void InstrumentPickerWindow_ScpiChoices_HasNoLayoutProblems(string theme) =>
        Review(theme, () => new InstrumentPickerWindow(new InstrumentPickerDialogViewModel(new InstrumentPickerScreenshotTests.GuideProvider())), 460, 340);
}
