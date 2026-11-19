using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Imaging.Analysis.Bursts;
using OoBDev.TestUtilities;

namespace OoBDev.Imaging.Analysis.Tests;

[TestClass]
public class QualityAnalyzerTests
{
    public required TestContext TestContext { get; set; }

    private const int Size = 128;

    private static byte[] Checker(int cell)
    {
        var gray = new byte[Size * Size];
        for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
                gray[(y * Size) + x] = (byte)((((x / cell) + (y / cell)) % 2 == 0) ? 20 : 230);
        return gray;
    }

    private static byte[] Blur(byte[] src, int radius)
    {
        var dst = new byte[src.Length];
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                double sum = 0;
                var n = 0;
                for (var dy = -radius; dy <= radius; dy++)
                    for (var dx = -radius; dx <= radius; dx++)
                    {
                        int yy = Math.Clamp(y + dy, 0, Size - 1), xx = Math.Clamp(x + dx, 0, Size - 1);
                        sum += src[(yy * Size) + xx];
                        n++;
                    }

                dst[(y * Size) + x] = (byte)(sum / n);
            }
        }

        return dst;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Analyze_BlurredImage_IsLessSharpThanTheOriginal()
    {
        var sharp = QualityAnalyzer.Analyze(Checker(4), Size, Size);
        var soft = QualityAnalyzer.Analyze(Blur(Checker(4), 2), Size, Size);

        TestContext.WriteLine("sharp focus {0:F1} hf {1:F4}; blurred focus {2:F1} hf {3:F4}", sharp.FocusTile, sharp.HfShare, soft.FocusTile, soft.HfShare);
        Assert.IsTrue(sharp.FocusTile > soft.FocusTile * 3);
        Assert.IsTrue(sharp.HfShare > soft.HfShare);
        Assert.IsTrue(sharp.Noise > soft.Noise);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Analyze_ConstantImage_HasNoFocusAndKeepsLuma()
    {
        var gray = new byte[Size * Size];
        Array.Fill(gray, (byte)100);

        var m = QualityAnalyzer.Analyze(gray, Size, Size);

        TestContext.WriteLine("{0}", m);
        Assert.AreEqual(0, m.FocusTile, 1e-9);
        Assert.AreEqual(100, m.LumaMean, 1e-9);
        Assert.AreEqual(0, m.DynRange, 1e-9);
        Assert.AreEqual(1.0, m.Isotropy, 1e-9);
        Assert.AreEqual(0, m.ClipShadowPct, 1e-9);
        Assert.AreEqual(0, m.ClipHighlightPct, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Analyze_HalfBlackHalfWhite_ReportsClippingAndRange()
    {
        var gray = new byte[Size * Size];
        for (var i = gray.Length / 2; i < gray.Length; i++)
            gray[i] = 255;

        var m = QualityAnalyzer.Analyze(gray, Size, Size);

        TestContext.WriteLine("{0}", m);
        Assert.AreEqual(50, m.ClipShadowPct, 0.01);
        Assert.AreEqual(50, m.ClipHighlightPct, 0.01);
        Assert.AreEqual(255, m.DynRange, 1e-9);
        Assert.AreEqual(127.5, m.LumaMean, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Analyze_VerticalStripes_AreAnisotropic()
    {
        var gray = new byte[Size * Size];
        for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
                gray[(y * Size) + x] = (byte)((x / 4 % 2) * 200);

        var m = QualityAnalyzer.Analyze(gray, Size, Size);

        TestContext.WriteLine("isotropy {0:F4}", m.Isotropy);
        Assert.IsTrue(m.Isotropy < 0.05);
        Assert.IsTrue(QualityAnalyzer.Analyze(Checker(4), Size, Size).Isotropy > 0.9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Analyze_TooSmallOrWrongLength_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => QualityAnalyzer.Analyze(new byte[16 * 16], 16, 16));
        Assert.ThrowsExactly<ArgumentException>(() => QualityAnalyzer.Analyze(new byte[10], 64, 64));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Analyze_NonSquareImage_Works()
    {
        var gray = new byte[200 * 40];
        for (var i = 0; i < gray.Length; i++)
            gray[i] = (byte)(i * 7 % 251);

        var m = QualityAnalyzer.Analyze(gray, 200, 40);

        Assert.IsTrue(m.FocusTile > 0);
        Assert.IsTrue(m.HfShare is >= 0 and <= 1);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Analyze_TransposedImageWithOddSides_GivesTheSameSpectrumShare()
    {
        const int w = 100, h = 60;
        var gray = new byte[w * h];
        var transposed = new byte[w * h];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var v = (byte)(((x * 37) + (y * 91) + (x * y)) % 251);
                gray[(y * w) + x] = v;
                transposed[(x * h) + y] = v;
            }
        }

        var a = QualityAnalyzer.Analyze(gray, w, h);
        var b = QualityAnalyzer.Analyze(transposed, h, w);

        TestContext.WriteLine("hf {0:F6} vs transposed {1:F6}", a.HfShare, b.HfShare);
        Assert.AreEqual(a.HfShare, b.HfShare, 1e-9, "rows and columns go through different transform lengths (neither a power of two)");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Analyze_SingleFrequencyStripes_PutsTheEnergyAtThatFrequency()
    {
        const int w = 96, h = 48;
        var gray = new byte[w * h];
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
                gray[(y * w) + x] = (byte)(128 + (100 * Math.Cos(2 * Math.PI * 24 * x / w)));

        var m = QualityAnalyzer.Analyze(gray, w, h);

        TestContext.WriteLine("hf {0:F4}", m.HfShare);
        Assert.IsGreaterThan(0.99, m.HfShare, "24 cycles over 96 pixels is half of Nyquist, above the 0.25 cut");
    }
}
