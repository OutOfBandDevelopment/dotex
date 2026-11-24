using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>What the stream monitor shows for a selected capture.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class StreamCapturePreviewTests
{
    private static StreamMonitorCapture Capture(StreamContentKind kind, byte[] data) =>
        new(new StreamCapture(kind, data, DateTimeOffset.UnixEpoch, StreamCaptureEnd.Complete, false), "dev", DateTimeOffset.UnixEpoch, null, "disk full");

    [TestMethod]
    public void Render_Svg_DrawsIt()
    {
        // Stage
        var capture = Capture(StreamContentKind.Svg, global::System.Text.Encoding.UTF8.GetBytes("<svg viewBox='0 0 4 4'><rect width='2' height='2'/></svg>"));

        // Test
        var image = StreamCapturePreview.Render(capture, out var message);

        // Assert
        Assert.IsNotNull(image);
        Assert.AreEqual(string.Empty, message);
    }

    [TestMethod]
    public void Render_BrokenSvg_ExplainsWhy()
    {
        // Stage / Test
        var image = StreamCapturePreview.Render(Capture(StreamContentKind.Svg, [(byte)'<']), out var message);

        // Assert
        Assert.IsNull(image);
        StringAssert.StartsWith(message, "Could not draw this SVG");
    }

    [TestMethod]
    public void Render_Png_Decodes()
    {
        // Stage
        var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

        // Test
        var image = StreamCapturePreview.Render(Capture(StreamContentKind.Png, png), out var message);

        // Assert
        Assert.IsNotNull(image, message);
        Assert.IsTrue(image.IsFrozen);
    }

    [TestMethod]
    public void Render_TruncatedPng_ExplainsWhy()
    {
        // Stage / Test
        var image = StreamCapturePreview.Render(Capture(StreamContentKind.Png, [0x89, 0x50, 0x4E]), out var message);

        // Assert
        Assert.IsNull(image);
        StringAssert.StartsWith(message, "Could not preview this PNG image");
    }

    [TestMethod]
    public void Render_Hpgl_DrawsTheScene()
    {
        // Stage / Test
        var image = StreamCapturePreview.Render(Capture(StreamContentKind.Hpgl, "IN;SP1;PU0,0;PD1000,1000;"u8.ToArray()), out var message);

        // Assert
        Assert.IsNotNull(image, message);
        Assert.AreEqual(1000, (int)image.Width);
    }

    [TestMethod]
    public void Render_PostScript_DrawsThePathOperators()
    {
        // Stage / Test
        var image = StreamCapturePreview.Render(Capture(StreamContentKind.PostScript, "%!PS\n10 10 moveto 100 100 lineto stroke showpage"u8.ToArray()), out var message);

        // Assert
        Assert.IsNotNull(image, message);
        Assert.AreEqual(string.Empty, message);
    }

    [TestMethod]
    public void Render_PostScriptWithoutLineWork_ExplainsWhy()
    {
        // Stage / Test
        var image = StreamCapturePreview.Render(Capture(StreamContentKind.PostScript, "%!PS"u8.ToArray()), out var message);

        // Assert
        Assert.IsNull(image);
        StringAssert.StartsWith(message, "No line work found in this PostScript document");
    }

    [TestMethod]
    public void Render_Pcl_DrawsTheEmbeddedHpgl()
    {
        // Stage
        var data = global::System.Text.Encoding.ASCII.GetBytes("\u001b%0BIN;SP1;PU0,0;PD1000,1000;\u001b%0A");

        // Test
        var image = StreamCapturePreview.Render(Capture(StreamContentKind.Pcl, data), out var message);

        // Assert
        Assert.IsNotNull(image, message);
    }
    [TestMethod]
    public async Task SelectedPostScript_NoToolInstalled_ExplainsWhatToInstall()
    {
        // Stage
        var directory = global::System.IO.Directory.CreateTempSubdirectory("devterm-nogs-").FullName;
        using var viewModel = new StreamMonitorDialogViewModel(new StreamMonitor(), previewRenderer: new ExternalPreviewRenderer([directory]));
        viewModel.AddCapture(Capture(StreamContentKind.PostScript, "%!PS"u8.ToArray()));

        // Test
        viewModel.SelectedIndex = 0;
        await viewModel.ExternalPreviewTask;

        // Assert
        Assert.IsNull(viewModel.Preview);
        StringAssert.Contains(viewModel.PreviewMessage, "Install Ghostscript");
        global::System.IO.Directory.Delete(directory);
    }

    [TestMethod]
    public void SelectedIndex_NothingCaptured_ShowsTheEmptyMessageAndCannotExport()
    {
        // Stage
        using var viewModel = new StreamMonitorDialogViewModel(new StreamMonitor(), exportDialog: null);

        // Test
        viewModel.SelectedIndex = 0;

        // Assert
        StringAssert.StartsWith(viewModel.PreviewMessage, "Nothing captured yet");
        Assert.IsFalse(viewModel.Export.CanExecute(null));
    }

    private static StreamMonitorCapture Named(string device, StreamContentKind kind, int size, int minute) =>
        new(new StreamCapture(kind, new byte[size], DateTimeOffset.UnixEpoch, StreamCaptureEnd.Complete, false), device, DateTimeOffset.UnixEpoch.AddMinutes(minute), null, null);

    private static StreamMonitorDialogViewModel Filled()
    {
        var viewModel = new StreamMonitorDialogViewModel(new StreamMonitor());
        viewModel.AddCapture(Named("scope", StreamContentKind.Png, 10, 1));
        viewModel.AddCapture(Named("printer", StreamContentKind.Hpgl, 30, 2));
        viewModel.AddCapture(Named("scope", StreamContentKind.Bmp, 20, 3));
        return viewModel;
    }

    [TestMethod]
    public void Search_DeviceName_NarrowsTheList()
    {
        // Stage
        using var viewModel = Filled();

        // Test
        viewModel.Search = "printer";

        // Assert
        Assert.AreEqual(1, viewModel.Captures.Count);
    }

    [TestMethod]
    public void PaneIndex_OneDevice_ShowsOnlyItsCaptures()
    {
        // Stage
        using var viewModel = Filled();
        CollectionAssert.AreEqual(new[] { "All devices (3)", "printer (1)", "scope (2)" }, viewModel.Panes);

        // Test
        viewModel.PaneIndex = 2;

        // Assert
        Assert.AreEqual(2, viewModel.Captures.Count);
    }

    [TestMethod]
    public void KindIndex_OneType_ShowsOnlyThatType()
    {
        // Stage
        using var viewModel = Filled();
        CollectionAssert.AreEqual(new[] { "All types", "BMP image", "HP-GL plot", "PNG image" }, viewModel.KindChoices);

        // Test
        viewModel.KindIndex = 1;

        // Assert
        Assert.AreEqual(1, viewModel.Captures.Count);
        StringAssert.Contains(viewModel.Captures[0], "BMP image");
    }

    [TestMethod]
    public void SortIndex_Largest_PutsTheBiggestFirst()
    {
        // Stage
        using var viewModel = Filled();

        // Test
        viewModel.SortIndex = viewModel.SortChoices.ToList().IndexOf(StreamCaptureSort.Largest);

        // Assert
        StringAssert.Contains(viewModel.Captures[0], "30 bytes");
    }

    [TestMethod]
    public void Export_SelectedCapture_WritesItWhereTheDialogSays()
    {
        // Stage
        using var exportFolder = new global::OoBDev.System.IO.TempDirectoryHandle();
        var target = global::System.IO.Path.Combine(exportFolder.DirectoryPath, "export.png");
        using var viewModel = new StreamMonitorDialogViewModel(new StreamMonitor(), new FixedExport(target));
        viewModel.AddCapture(Named("scope", StreamContentKind.Png, 7, 1));
        viewModel.SelectedIndex = 0;

        // Test
        viewModel.Export.Execute(null);

        // Assert
        Assert.AreEqual(7, global::System.IO.File.ReadAllBytes(target).Length);
    }

    [TestMethod]
    public void OpenFolder_ExportDialog_OpensTheSaveFolder()
    {
        // Stage
        var export = new FixedExport("x");
        using var viewModel = new StreamMonitorDialogViewModel(new StreamMonitor(), export);

        // Test
        viewModel.OpenFolder.Execute(null);

        // Assert
        Assert.IsNotNull(export.Opened);
        StringAssert.StartsWith(viewModel.FolderText, "Saving to");
    }

    [TestMethod]
    public async global::System.Threading.Tasks.Task ConvertSelectedAsync_Hpgl_WritesAnSvgBesideIt()
    {
        // Stage
        using var folder = new global::OoBDev.System.IO.TempDirectoryHandle();
        var directory = folder.DirectoryPath;
        var source = global::System.IO.Path.Combine(directory, "plot.hpgl");
        var data = global::System.Text.Encoding.ASCII.GetBytes("IN;SP1;PU0,0;PD1000,1000;");
        global::System.IO.File.WriteAllBytes(source, data);
        var capture = new StreamMonitorCapture(new StreamCapture(StreamContentKind.Hpgl, data, DateTimeOffset.UnixEpoch, StreamCaptureEnd.Complete, false), "plotter", DateTimeOffset.UnixEpoch, source, null);
        var options = new StreamCaptureConverterOptions { Mode = StreamConversionMode.InternalHpglToSvg };
        using var viewModel = new StreamMonitorDialogViewModel(new StreamMonitor(), converterOptions: options);
        viewModel.AddCapture(capture);
        viewModel.SelectedIndex = 0;

        // Test
        await viewModel.ConvertSelectedAsync();

        // Assert
        Assert.IsTrue(global::System.IO.File.Exists(global::System.IO.Path.Combine(directory, "plot.svg")), viewModel.ExportStatus);
        StringAssert.StartsWith(viewModel.ExportStatus, "Converted to plot.svg");
    }

    [TestMethod]
    public void Convert_NoConverterOptions_CannotExecute()
    {
        // Stage
        using var viewModel = Filled();
        viewModel.SelectedIndex = 0;

        // Assert
        Assert.IsFalse(viewModel.Convert.CanExecute(null));
    }

    private sealed class FixedExport(string path) : IStreamExportDialog
    {
        public string? Opened { get; private set; }

        public string? PickExportPath(string suggestedName, string extension, string kindName) => path;

        public void OpenFolder(string directory) => Opened = directory;
    }
}
