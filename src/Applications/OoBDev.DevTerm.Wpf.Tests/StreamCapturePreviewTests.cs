using System;
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
}
