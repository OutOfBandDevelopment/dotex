using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Kuando.Busylight.Panels.Tests;


[TestClass]
[TestCategory(TestCategories.Hid)]
[TestCategory(TestCategories.Kuando_Busylight)]
public sealed class BusylightControlSurfaceTests
{
    private static (BusylightControlSurface Surface, List<byte[]> Sent) CreateSurface()
    {
        var sent = new List<byte[]>();
        return (new BusylightControlSurface((data, _) =>
        {
            sent.Add(data.ToArray());
            return Task.CompletedTask;
        }), sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_SetColorRedThenApply_SendsRedFrameWithDefaultTimingAndAudio()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("color", "Red", TestContext.CancellationToken);
        await surface.InvokeAsync("apply", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x01, 0x00, 0x80 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_SetColorBlue_UsesCorrectRgbByteOrder()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("color", "Blue", TestContext.CancellationToken);
        await surface.InvokeAsync("apply", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x01, 0x00, 0x80 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_BlinkModeSlow_SetsOnOffPreset()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("blinkMode", "Slow", TestContext.CancellationToken);
        await surface.InvokeAsync("apply", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x50, 0x50, 0x80 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_ExplicitOnOffBytes_OverrideDefaults()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("onMs", "10", TestContext.CancellationToken);
        await surface.InvokeAsync("offMs", "20", TestContext.CancellationToken);
        await surface.InvokeAsync("apply", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 10, 20, 0x80 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_Mute_ClearsThePlayBitInTheAudioByte()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("mute", "1", TestContext.CancellationToken);
        await surface.InvokeAsync("apply", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_TrackAndVolume_PackIntoTheAudioByte()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("track", "Nordic", TestContext.CancellationToken);
        await surface.InvokeAsync("volume", "5", TestContext.CancellationToken);
        await surface.InvokeAsync("apply", null, TestContext.CancellationToken);

        // Play bit set (not muted) | track index 1 (Nordic) << 3 | volume 5 = 0x80 | 0x08 | 0x05.
        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x8D }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_CustomColor_IsANoOp()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("customColor", null, TestContext.CancellationToken);

        Assert.IsEmpty(sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_ProgramSequence_IsANoOp()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("programSequence", null, TestContext.CancellationToken);

        Assert.IsEmpty(sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_UnknownCommand_Throws()
    {
        var (surface, _) = CreateSurface();

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => surface.InvokeAsync("notARealCommand", null, TestContext.CancellationToken));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task PreviewCommand_Apply_ShowsTheFrameApplyWouldSendAsHex()
    {
        var (surface, sent) = CreateSurface();
        await surface.InvokeAsync("color", "Red", TestContext.CancellationToken);

        var preview = surface.PreviewCommand("apply", null);
        await surface.InvokeAsync("apply", null, TestContext.CancellationToken);

        Assert.AreEqual("00 00 00 FF 00 00 01 00 80", preview);
        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x01, 0x00, 0x80 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void PreviewCommand_StateOnlySetters_AreNull()
    {
        var (surface, _) = CreateSurface();

        Assert.IsNull(surface.PreviewCommand("color", "Red"));
        Assert.IsNull(surface.PreviewCommand("volume", "3"));
        Assert.IsNull(surface.PreviewCommand("customColor", null));
    }

    [TestCategory(TestCategories.BugRegression)]
    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_OnMsWithNonNumericValue_DoesNotThrow_AndFallsBackToZero()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("onMs", "not-a-number", TestContext.CancellationToken);
        await surface.InvokeAsync("apply", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, sent[^1]);
    }

    public required TestContext TestContext { get; set; }
}
