using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Imaging.Analysis.Tests;

[TestClass]
public class AnalysisTests
{
    private static RgbImage Solid(byte r, byte g, byte b, int w = 4, int h = 4)
    {
        var buffer = new byte[w * h * 3];
        for (var i = 0; i < buffer.Length; i += 3)
        {
            buffer[i] = r;
            buffer[i + 1] = g;
            buffer[i + 2] = b;
        }

        return new RgbImage(w, h, buffer);
    }

    private static RgbImage Pixels(params (byte R, byte G, byte B)[] px) =>
        new(px.Length, 1, px.SelectMany(p => new[] { p.R, p.G, p.B }).ToArray());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void RgbImage_BadArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RgbImage(0, 1, new byte[3]));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RgbImage(1, 0, new byte[3]));
        Assert.ThrowsExactly<ArgumentException>(() => new RgbImage(2, 1, new byte[3]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Histogram_SolidGrey_HasOneBinPerChannel()
    {
        var h = ImageHistograms.Compute(Solid(128, 128, 128));

        Assert.AreEqual(16, h.Red[128]);
        Assert.AreEqual(16, h.Green.Sum());
        Assert.AreEqual(1, h.Luminance.Count(c => c > 0));
        Assert.AreEqual(16, h.Luminance[128]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Histogram_FewBins_SplitTheFullRange()
    {
        var h = ImageHistograms.Compute(Pixels((0, 0, 0), (255, 255, 255), (127, 127, 127), (128, 128, 128)), 2);

        CollectionAssert.AreEqual(new[] { 2, 2 }, h.Red.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Histogram_BadArguments_Throw()
    {
        var image = Solid(0, 0, 0);

        Assert.ThrowsExactly<ArgumentNullException>(() => ImageHistograms.Compute(null!));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ImageHistograms.Compute(image, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ImageHistograms.Compute(image, 257));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Exposure_TenPercentWhite_Clips()
    {
        var px = Enumerable.Repeat(((byte)100, (byte)100, (byte)100), 9).Concat([((byte)255, (byte)255, (byte)255)]).ToArray();

        var e = ExposureMetrics.Compute(Pixels(px));

        Assert.AreEqual(10.0, e.ClippedWhitePct, 1e-9);
        Assert.AreEqual(0.0, e.ClippedBlackPct);
        Assert.AreEqual(115.5, e.MeanLuminance, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Exposure_HalfBlackHalfWhite_HasMaximumContrast()
    {
        var e = ExposureMetrics.Compute(Pixels((0, 0, 0), (255, 255, 255)));

        Assert.AreEqual(127.5, e.MeanLuminance, 1e-9);
        Assert.AreEqual(127.5, e.Contrast, 1e-9);
        Assert.AreEqual(50.0, e.ClippedBlackPct);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Exposure_ThresholdsWiden_AndSolidHasNoContrast()
    {
        var e = ExposureMetrics.Compute(Solid(250, 250, 250), blackThreshold: 5, whiteThreshold: 250);

        Assert.AreEqual(100.0, e.ClippedWhitePct);
        Assert.AreEqual(0.0, e.Contrast, 1e-9);
        Assert.ThrowsExactly<ArgumentNullException>(() => ExposureMetrics.Compute(null!));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow((byte)255, (byte)0, (byte)0, 0.0)]
    [DataRow((byte)0, (byte)255, (byte)0, 120.0)]
    [DataRow((byte)0, (byte)0, (byte)255, 240.0)]
    public void Facets_PureColour_HasItsHue(byte r, byte g, byte b, double hue)
    {
        var f = ColorFacets.Compute(Solid(r, g, b));

        Assert.AreEqual(hue, f.DominantHue!.Value, 1e-6);
        Assert.AreEqual(1.0, f.MeanSaturation, 1e-9);
        Assert.AreEqual(1.0, f.MeanValue, 1e-9);
        Assert.AreEqual(1, f.Palette.Count);
        Assert.AreEqual(1.0, f.Palette[0].Share, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Facets_Grey_HasNoHue()
    {
        var f = ColorFacets.Compute(Solid(100, 100, 100));

        Assert.IsNull(f.DominantHue);
        Assert.AreEqual(0.0, f.MeanSaturation);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Facets_RedAndGreen_PaletteSplitsAndIsDeterministic()
    {
        var image = Pixels((255, 0, 0), (255, 0, 0), (255, 0, 0), (0, 255, 0));

        var a = ColorFacets.Compute(image, 2);
        var b = ColorFacets.Compute(image, 2);

        Assert.AreEqual(2, a.Palette.Count);
        Assert.AreEqual(new PaletteColor(255, 0, 0, 0.75), a.Palette[0]);
        Assert.AreEqual(new PaletteColor(0, 255, 0, 0.25), a.Palette[1]);
        CollectionAssert.AreEqual(a.Palette.ToArray(), b.Palette.ToArray());
        Assert.IsTrue(a.DominantHue is > 0 and < 120);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Facets_MoreColoursThanPalette_AreMergedAndSharesSumToOne()
    {
        var px = Enumerable.Range(0, 30).Select(i => ((byte)(i * 8), (byte)(255 - (i * 8)), (byte)(i * 3))).ToArray();

        var f = ColorFacets.Compute(Pixels(px), 4);

        Assert.AreEqual(4, f.Palette.Count);
        Assert.AreEqual(1.0, f.Palette.Sum(c => c.Share), 1e-9);
        Assert.IsTrue(f.Palette.Zip(f.Palette.Skip(1)).All(p => p.First.Share >= p.Second.Share));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Facets_PaletteLargerThanDistinctColours_StopsEarly()
    {
        var f = ColorFacets.Compute(Pixels((1, 2, 3), (4, 5, 6)), 8);

        Assert.AreEqual(2, f.Palette.Count);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ColorFacets.Compute(Solid(0, 0, 0), 0));
        Assert.ThrowsExactly<ArgumentNullException>(() => ColorFacets.Compute(null!));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Transforms_ReturnNewBuffersAndLeaveTheInput()
    {
        var source = Pixels((255, 0, 0), (10, 20, 30));

        var gray = source.ToGrayscale();
        var adjusted = source.AdjustBrightnessContrast(-20, 2);

        Assert.AreEqual(54, gray.Rgb.Span[0]);
        Assert.AreEqual(gray.Rgb.Span[0], gray.Rgb.Span[2]);
        Assert.AreEqual(255, source.Rgb.Span[0]);
        Assert.AreEqual(255, adjusted.Rgb.Span[0]);
        Assert.AreEqual(0, adjusted.Rgb.Span[1]);
        Assert.AreEqual(0, adjusted.Rgb.Span[3]);
        Assert.AreEqual(20, adjusted.Rgb.Span[4]);
    }
}
