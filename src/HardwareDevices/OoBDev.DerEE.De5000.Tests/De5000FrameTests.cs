using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.DerEE.De5000.Tests;

[TestClass]
public class De5000FrameTests
{
    // header 00 0D | flags 00 | freq/tol 40 00 | primary 01 04 D2 33 00 | secondary 01 00 0C 03 00 | footer 0D 0A
    internal static readonly byte[] Valid =
    [
        0x00, 0x0D, 0x00, 0x40, 0x00,
        0x01, 0x04, 0xD2, 0x33, 0x00,
        0x01, 0x00, 0x0C, 0x03, 0x00,
        0x0D, 0x0A,
    ];

    private static byte[] With(Action<byte[]> change)
    {
        var bytes = (byte[])Valid.Clone();
        change(bytes);
        return bytes;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_ValidPacket_DecodesAllFields()
    {
        Assert.IsTrue(De5000Frame.TryParse(Valid, out var frame));
        Assert.AreEqual("Ls", frame.PrimaryQuantity);
        Assert.AreEqual(1.234, frame.PrimaryValue, 1e-9);
        Assert.AreEqual("mH", frame.PrimaryUnit);
        Assert.AreEqual("normal", frame.PrimaryStatus);
        Assert.AreEqual("D", frame.SecondaryQuantity);
        Assert.AreEqual(0.012, frame.SecondaryValue, 1e-9);
        Assert.AreEqual(string.Empty, frame.SecondaryUnit);
        Assert.AreEqual("1 kHz", frame.Frequency);
        Assert.IsFalse(frame.Hold);
        Assert.IsFalse(frame.Parallel);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_WrongLengthHeaderOrFooter_ReturnsFalse()
    {
        Assert.IsFalse(De5000Frame.TryParse(Valid.AsSpan(0, 16), out _));
        Assert.IsFalse(De5000Frame.TryParse(With(b => b[0] = 0xFF), out _));
        Assert.IsFalse(De5000Frame.TryParse(With(b => b[16] = 0x0D), out _));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_AllFlagsSet_SetsEveryFlag()
    {
        Assert.IsTrue(De5000Frame.TryParse(With(b => b[2] = 0xFF), out var f));
        Assert.IsTrue(f.Hold && f.ReferenceShown && f.Delta && f.Calibration && f.Sorting && f.LcrAuto && f.AutoRange && f.Parallel);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_ParallelFlag_UsesTheParallelQuantityTable()
    {
        Assert.IsTrue(De5000Frame.TryParse(With(b => b[2] = 0b1000_0000), out var f));
        Assert.AreEqual("Lp", f.PrimaryQuantity);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_UnknownCodes_AreNull()
    {
        Assert.IsTrue(De5000Frame.TryParse(With(b => { b[5] = 0xFF; b[3] = 0b1100_0000; }), out var f));
        Assert.IsNull(f.PrimaryQuantity);
        Assert.IsNull(f.Frequency);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_SecondaryStatus_UsesThreeBits()
    {
        Assert.IsTrue(De5000Frame.TryParse(With(b => b[14] = 0xFF), out var f));
        Assert.AreEqual("PASS", f.SecondaryStatus);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_NegativeSecondaryDegrees_IsNegative()
    {
        Assert.IsTrue(De5000Frame.TryParse(With(b => { b[10] = 0x04; b[11] = 0xFE; b[12] = 0x3E; b[13] = 0x71; }), out var f));
        Assert.AreEqual("Theta", f.SecondaryQuantity);
        Assert.AreEqual(-45.0, f.SecondaryValue, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_NegativeSecondaryPercent_IsNegative()
    {
        Assert.IsTrue(De5000Frame.TryParse(With(b => { b[11] = 0xFF; b[12] = 0xF4; b[13] = 0x69; }), out var f));
        Assert.AreEqual("%", f.SecondaryUnit);
        Assert.AreEqual(-1.2, f.SecondaryValue, 1e-9);
    }
}
