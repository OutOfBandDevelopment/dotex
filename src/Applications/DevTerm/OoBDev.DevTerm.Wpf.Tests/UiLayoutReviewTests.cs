using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.DevTerm.DeviceManifests.Editing;

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
    public TestContext TestContext { get; set; } = null!;

    [TestCleanup]
    public void ResetTheme() => ActiveTheme.Reset();

    private static readonly LayoutAllowance[] _dockGlyphs =
    [
        new(WpfLayoutAssert.LightChromeCheck, e => e is global::System.Windows.Shapes.Path && e.Name is "PART_ImgHidePin" or "MenuDropDownButtonImage", "AvalonDock's pin and menu glyphs are light icons drawn on its dark header, on purpose"),
    ];

    private static void Review(string theme, Func<Window> build, double width, double height, global::System.Collections.Generic.IReadOnlyList<LayoutAllowance>? allowed = null)
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
                WpfLayoutAssert.Check(window, new LayoutCheckOptions { IsDarkTheme = theme == BuiltInThemes.DarkName, Allowed = allowed ?? [] }, window.GetType().Name + " (" + theme + ")");
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
        Review(theme, ShellScreenshotTests.BuildWindow, 900, 650, _dockGlyphs);

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
                return new ConnectionEditorWindow(editor);
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
    public void ManifestEditorWindow_NewManifest_HasNoLayoutProblems(string theme) =>
        Review(
            theme,
            () => new ManifestEditorWindow(new ManifestEditorDialogViewModel(new ManifestEditorViewModel(TempDirectory()), new ManifestEditorDialogTests.FakeFiles())),
            1100,
            700);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void PlaybackWindow_PartWayThroughWithANote_HasNoLayoutProblems(string theme)
    {
        var directory = Directory.CreateTempSubdirectory("devterm-playback-review-");
        try
        {
            Review(theme, () => PlaybackScreenshotTests.BuildWindow(PlaybackScreenshotTests.WriteSampleLog(directory.FullName)), 1000, 560);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public async Task MergedLogWindow_TwoDevices_HasNoLayoutProblems(string theme)
    {
        var (log, sessions) = await MergedLogScreenshotTests.BuildLogAsync(TestContext.CancellationToken);
        await using var _ = sessions;

        Review(theme, () => new MergedLogWindow(new MergedLogDialogViewModel(log)), 700, 360);
    }

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void InstrumentPickerWindow_ScpiChoices_HasNoLayoutProblems(string theme) =>
        Review(theme, () => new InstrumentPickerWindow(new InstrumentPickerDialogViewModel(new InstrumentPickerScreenshotTests.GuideProvider())), 460, 340);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void MainWindow_WithScpiControlPanel_HasNoLayoutProblems(string theme) =>
        Review(theme, BuildWindowWithScpiPanel, 900, 650, _dockGlyphs);

    [TestMethod]
    [DataRow(BuiltInThemes.LightName)]
    [DataRow(BuiltInThemes.DarkName)]
    public void SessionLogNotePrompt_CodeBuilt_HasNoLayoutProblems(string theme) =>
        Review(theme, () => WpfSessionLogDialog.BuildNoteWindow(out _), 420, 140);

    private static MainWindow BuildWindowWithScpiPanel()
    {
        var profile = new OoBDev.Scpi.ScpiInstrumentProfile
        {
            Name = "Bench PSU",
            Terminator = "\n",
            Notes = "A long note about the instrument that has to wrap inside the Notes section of the panel without being clipped or running off the window.",
            Commands =
            [
                new OoBDev.Scpi.ScpiCommandDefinition { Id = "idn", Label = "Identify", Category = "System", Template = "*IDN?", IsQuery = true },
                new OoBDev.Scpi.ScpiCommandDefinition { Id = "rst", Label = "Reset", Category = "System", Template = "*RST" },
                new OoBDev.Scpi.ScpiCommandDefinition { Id = "volt", Label = "Read voltage", Category = "Measure", Template = "MEAS:VOLT?", IsQuery = true },
                new OoBDev.Scpi.ScpiCommandDefinition { Id = "curr", Label = "Read current", Category = "Measure", Template = "MEAS:CURR?", IsQuery = true },
            ],
        };
        var shell = new ShellViewModel([new ScpiPanelFactory(profile, _ => Task.FromResult<string?>("ACME"))]);
        return new MainWindow { DataContext = shell };
    }
}
