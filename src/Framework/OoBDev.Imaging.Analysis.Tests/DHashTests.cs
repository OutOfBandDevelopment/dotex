using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Imaging.Analysis.Bursts;
using OoBDev.TestUtilities;

namespace OoBDev.Imaging.Analysis.Tests;

[TestClass]
public class DHashTests
{
    private static byte[] Gradient(int w, int h, bool descending)
    {
        var gray = new byte[w * h];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
                gray[(y * w) + x] = (byte)((descending ? w - 1 - x : x) * 255 / (w - 1));
        }

        return gray;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Compute_ConstantImage_IsZero() => Assert.AreEqual(0L, DHash.Compute(new byte[64 * 48], 64, 48));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Compute_DescendingGradient_SetsEveryBit() => Assert.AreEqual(-1L, DHash.Compute(Gradient(90, 60, true), 90, 60));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Compute_AscendingGradient_IsZero() => Assert.AreEqual(0L, DHash.Compute(Gradient(90, 60, false), 90, 60));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Compute_SameImageTwice_IsEqualAndUpscaleWorks()
    {
        var small = Gradient(5, 3, true);

        Assert.AreEqual(DHash.Compute(small, 5, 3), DHash.Compute(small, 5, 3));
        Assert.AreEqual(-1L, DHash.Compute(small, 5, 3));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Compute_HalfBrightLeft_SetsTheEdgeAndRingingBits()
    {
        var gray = new byte[18 * 8];
        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 9; x++)
                gray[(y * 18) + x] = 255;
        }

        var hash = DHash.Compute(gray, 18, 8);

        Assert.AreEqual(32, DHash.Hamming(hash, 0));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Hamming_CountsDifferingBits()
    {
        Assert.AreEqual(0, DHash.Hamming(5, 5));
        Assert.AreEqual(2, DHash.Hamming(0b1010, 0b0000));
        Assert.AreEqual(64, DHash.Hamming(0, -1));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Gray_FromRgbaAndRgb_UseTheSameWeights()
    {
        byte[] rgb = [255, 0, 0, 0, 255, 0, 0, 0, 255, 255, 255, 255];
        byte[] rgba = [255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255];

        var a = DHash.GrayFromRgb(rgb, 4, 1);
        var b = DHash.GrayFromRgba(rgba, 4, 1);

        CollectionAssert.AreEqual(new byte[] { 76, 150, 29, 255 }, a);
        CollectionAssert.AreEqual(a, b);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void BadArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => DHash.Compute(null!, 1, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => DHash.Compute(new byte[1], 0, 1));
        Assert.ThrowsExactly<ArgumentException>(() => DHash.Compute(new byte[3], 2, 2));
        Assert.ThrowsExactly<ArgumentException>(() => DHash.GrayFromRgb(new byte[5], 2, 1));
        Assert.ThrowsExactly<ArgumentException>(() => DHash.GrayFromRgba(new byte[5], 1, 1));
    }
}
