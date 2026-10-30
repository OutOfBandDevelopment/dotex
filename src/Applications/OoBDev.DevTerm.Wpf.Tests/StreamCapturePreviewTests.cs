using System;
using System.Linq;
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
    public void Render_Hpgl_SaysNoPreview()
    {
        // Stage / Test
        var image = StreamCapturePreview.Render(Capture(StreamContentKind.Hpgl, [1]), out var message);

        // Assert
        Assert.IsNull(image);
        StringAssert.StartsWith(message, "Preview not available yet for HP-GL plot");
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
        var target = global::System.IO.Path.Combine(global::System.IO.Path.GetTempPath(), $"export-{Guid.NewGuid():N}.png");
        using var viewModel = new StreamMonitorDialogViewModel(new StreamMonitor(), new FixedExport(target));
        viewModel.AddCapture(Named("scope", StreamContentKind.Png, 7, 1));
        viewModel.SelectedIndex = 0;

        // Test
        viewModel.Export.Execute(null);

        // Assert
        Assert.AreEqual(7, global::System.IO.File.ReadAllBytes(target).Length);
        global::System.IO.File.Delete(target);
    }

    private sealed class FixedExport(string path) : IStreamExportDialog
    {
        public string? PickExportPath(string suggestedName, string extension, string kindName) => path;
    }
}
