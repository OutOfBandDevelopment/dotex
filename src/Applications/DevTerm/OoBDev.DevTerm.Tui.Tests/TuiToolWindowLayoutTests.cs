using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OoBDev.System.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm;
using OoBDev.DevTerm.DeviceManifests.Editing;
using OoBDev.DevTerm.Logging;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// Layout regression tests (see <see cref="TuiLayoutAssert"/>) for the TUI's tool windows: the
/// device manifest editor (<see cref="ManifestEditorMode"/> — one outline node of every kind, the live
/// preview, and a brand-new manifest's empty Panel node), Playback (<see cref="PlaybackMode"/>) and the
/// Stream Monitor (<see cref="StreamMonitorMode"/>), at each review size and in both themes. Each also
/// saves a review PNG under <c>artifacts/ui-review/tui/</c>.
/// </summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class TuiToolWindowLayoutTests
{
    private static readonly string _bundled = Path.Combine(AppContext.BaseDirectory, "manifests", "loopback-sensor-demo");
    private static TempDirectoryHandle? _folder;
    private static string _directory = string.Empty;
    private static ManifestEditorParts? _editor;

    [ClassInitialize]
    public static void CreateDirectory(TestContext context)
    {
        _folder = new TempDirectoryHandle();
        _directory = _folder.DirectoryPath;
    }

    [ClassCleanup]
    public static void DeleteDirectory() => _folder?.Dispose();

    [TestCleanup]
    public void Cleanup() => TuiReview.ResetTheme();

    public static IEnumerable<object[]> SizesAndThemes =>
        from theme in new[] { "light", "dark" }
        from size in TuiReview.Sizes
        select new object[] { size.Width, size.Height, theme };

    private static Func<IApplication, View> Editor() => app =>
    {
        var editor = new ManifestEditorViewModel(Path.Combine(_directory, "user-manifests"), Path.Combine(AppContext.BaseDirectory, "manifests"));
        Assert.IsTrue(editor.Open(_bundled), editor.StatusMessage);
        _editor = ManifestEditorMode.BuildWindow(app, editor);
        return _editor.Window;
    };

    private static Func<IApplication, View> NewManifestEditor() => app =>
    {
        var editor = new ManifestEditorViewModel(Path.Combine(_directory, "user-manifests"), Path.Combine(AppContext.BaseDirectory, "manifests"));
        editor.New();
        _editor = ManifestEditorMode.BuildWindow(app, editor);
        return _editor.Window;
    };

    /// <summary>One outline entry of every kind (for controls, of every control type): the first of each.</summary>
    private static IEnumerable<(string Slug, int Index)> RepresentativeNodes(ManifestEditorViewModel editor)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < editor.Nodes.Count; i++)
        {
            var node = editor.Nodes[i];
            var kind = node.Kind == ManifestNodeKind.Control ? $"control-{node.Display.Trim().Split(':')[0]}" : node.Kind.ToString().ToLowerInvariant();
            if (seen.Add(kind))
            {
                yield return (kind.ToLowerInvariant(), i);
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void ManifestEditor_EveryKindOfOutlineNode(int width, int height, string theme)
    {
        var probe = new ManifestEditorViewModel(Path.Combine(_directory, "user-manifests"), Path.Combine(AppContext.BaseDirectory, "manifests"));
        Assert.IsTrue(probe.Open(_bundled), probe.StatusMessage);
        var nodes = RepresentativeNodes(probe).ToList();
        Assert.IsGreaterThanOrEqualTo(9, nodes.Count, "Identity, both headings, a command, a parameter, a pattern, the panel, a section and a control at least.");

        foreach (var (slug, index) in nodes)
        {
            TuiReview.Screen($"manifest-editor-{slug}", width, height, theme, Editor(), (app, _) =>
            {
                _editor!.Outline.SelectedItem = index;
            });
        }
    }

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void ManifestEditor_Preview(int width, int height, string theme) =>
        TuiReview.Screen("manifest-editor-preview", width, height, theme, Editor(), (app, _) => _editor!.ShowPreview());

    /// <summary>The device panel a manifest produces (bar graph, strip chart, vector displays, buttons) at each size and theme.</summary>
    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void ManifestPanel_LoopbackSensorDemo(int width, int height, string theme)
    {
        var manifest = OoBDev.DevTerm.DeviceManifests.DeviceManifestLoader.Load(_bundled);
        var transport = new OoBDev.DevTerm.Loopback.LoopbackTransport(Microsoft.Extensions.Options.Options.Create(new OoBDev.DevTerm.Loopback.LoopbackTransportOptions()));
        var session = new Session(transport, new Pipeline([]));
        session.OpenAsync().GetAwaiter().GetResult();
        try
        {
            using var panel = OoBDev.DevTerm.DeviceManifests.ManifestPanel.Attach(session, manifest);
            TuiReview.Screen("manifest-panel", width, height, theme, app => ManifestPanelMode.BuildWindow(app, panel).Window);
        }
        finally
        {
            session.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    /// <summary>A brand-new manifest's Panel node has no form yet: the hint text plus the "Create panel
    /// from commands" button, not reviewed by eye before now.</summary>
    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void ManifestEditor_NewManifest_ShowsCreatePanelHint(int width, int height, string theme) =>
        TuiReview.Screen("manifest-editor-new", width, height, theme, NewManifestEditor(), (app, _) =>
        {
            var panelIndex = _editor!.ViewModel.Nodes.ToList().FindIndex(n => n.Kind == ManifestNodeKind.Panel);
            Assert.IsGreaterThanOrEqualTo(0, panelIndex, "A new manifest should still have a Panel outline node.");
            _editor.Outline.SelectedItem = panelIndex;
        });

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void Playback_PartWayThroughWithANote(int width, int height, string theme)
    {
        var controller = new PlaybackPresenters().Open(PlaybackModeTests.WriteSampleLog(_directory, $"layout-{width}x{height}-{theme}.jsonl"), new ManualTimeProvider());
        PlaybackWindowParts? parts = null;
        TuiReview.Screen("playback", width, height, theme, app =>
        {
            parts = PlaybackMode.BuildWindow(app, controller);
            return parts.Window;
        }, (app, _) =>
        {
            parts!.Do(() => controller.SetPresenters(["ascii", "hex"]));
            parts.Do(() => controller.SeekTo(5));
            parts.Do(() => controller.AddNote("IDN reply is correct"));
            parts.Do(controller.Step);
            controller.MarkIn();
        });
    }

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public async Task StreamMonitor_WithCaptures(int width, int height, string theme)
    {
        await using var bench = await StreamMonitorBench.StartAsync("Rigol DG1062Z", Path.Combine(_directory, "exports"));
        await bench.CaptureAsync(StreamContentSamples.Png());
        bench.Time.Advance(TimeSpan.FromSeconds(41));
        await bench.CaptureAsync(StreamContentSamples.Hpgl());

        TuiReview.Screen("stream-monitor", width, height, theme, app => StreamMonitorMode.BuildWindow(app, bench.Monitor).Window);
    }
}
