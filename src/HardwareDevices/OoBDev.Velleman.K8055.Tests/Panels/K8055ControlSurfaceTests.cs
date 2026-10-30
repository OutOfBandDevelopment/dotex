using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Velleman.K8055.Panels.Tests;

[TestClass]
public sealed class K8055ControlSurfaceTests
{
    private static (K8055ControlSurface Surface, List<byte[]> Sent) CreateSurface()
    {
        var sent = new List<byte[]>();
        return (new K8055ControlSurface((data, _) =>
        {
            sent.Add(data.ToArray());
            return Task.CompletedTask;
        }), sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_TogglingDigitalOut1On_SendsSetOutputsFrameWithBit0Set()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("digitalOut1", "1", TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x05, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_TogglingDigitalOut8On_SetsBit7()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("digitalOut8", "1", TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x05, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_TwoDigitalOutMutations_SendsCombinedStateOnTheSecond()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("digitalOut1", "1", TestContext.CancellationToken);
        await surface.InvokeAsync("digitalOut3", "1", TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x05, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_TogglingDigitalOutOff_ClearsItsBit()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("digitalOut1", "1", TestContext.CancellationToken);
        await surface.InvokeAsync("digitalOut1", "0", TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_SetAnalogOut1_SendsValueInFrame()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("analogOut1", "128", TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x05, 0x00, 128, 0x00, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_SetAnalogOut2_PreservesPriorAnalogOut1()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("analogOut1", "128", TestContext.CancellationToken);
        await surface.InvokeAsync("analogOut2", "64", TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x05, 0x00, 128, 64, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_ResetCounter1_SendsFixedFrame()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("resetCounter1", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_ResetCounter2_SendsFixedFrame()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("resetCounter2", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_UnknownCommand_Throws()
    {
        var (surface, _) = CreateSurface();

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => surface.InvokeAsync("notARealCommand", null, TestContext.CancellationToken));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void PreviewCommand_DigitalOut_ShowsTheReportAsHex()
    {
        var (surface, _) = CreateSurface();

        Assert.AreEqual("00 05 01 00 00 00 00 00 00", surface.PreviewCommand("digitalOut1", "1"));
        Assert.AreEqual("00 05 00 FF 00 00 00 00 00", surface.PreviewCommand("analogOut1", "255"));
        Assert.AreEqual("00 03 00 00 00 00 00 00 00", surface.PreviewCommand("resetCounter1", null));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task PreviewCommand_NeitherSendsNorChangesState_AndMatchesTheNextInvoke()
    {
        var (surface, sent) = CreateSurface();
        await surface.InvokeAsync("digitalOut3", "1", TestContext.CancellationToken);

        // Previewing channel 1 on must not latch it: the next real send only has channel 2 added.
        _ = surface.PreviewCommand("digitalOut1", "1");
        var preview = surface.PreviewCommand("digitalOut2", "1");
        await surface.InvokeAsync("digitalOut2", "1", TestContext.CancellationToken);

        Assert.AreEqual("00 05 06 00 00 00 00 00 00", preview);
        Assert.HasCount(2, sent);
        CollectionAssert.AreEqual(new byte[] { 0x00, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void PreviewCommand_UnknownCommand_IsNull()
    {
        var (surface, _) = CreateSurface();

        Assert.IsNull(surface.PreviewCommand("notARealCommand", null));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void PreviewCommand_NonNumericValue_FallsBackToZeroRatherThanNull()
    {
        var (surface, _) = CreateSurface();

        Assert.AreEqual("00 05 00 00 00 00 00 00 00", surface.PreviewCommand("analogOut1", "abc"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_AnalogOutWithNonNumericValue_DoesNotThrow_AndFallsBackToZero()
    {
        var (surface, sent) = CreateSurface();

        await surface.InvokeAsync("analogOut1", "not-a-number", TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, sent[^1]);
    }

    public required TestContext TestContext { get; set; }
}
