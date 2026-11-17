using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Media.Metadata.Raw;
using OoBDev.TestUtilities;

namespace OoBDev.ImageMagick.Tests;

[TestClass]
public class MagickRawDeveloperTests
{
    public TestContext TestContext { get; set; } = null!;

    private static MagickImage Grey() => new(new MagickColor(100 * 257, 100 * 257, 100 * 257), 4, 4);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Render_Png16_SetsFormatAndDepth()
    {
        using var image = Grey();

        MagickRawDeveloper.Render(image, new RawDevelopOptions(Format: RawOutputFormat.Png, BitDepth: 16));

        Assert.AreEqual(MagickFormat.Png, image.Format);
        Assert.AreEqual(16u, image.Depth);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Render_JpegQuality_ClampedAndApplied()
    {
        using var image = Grey();

        MagickRawDeveloper.Render(image, new RawDevelopOptions(JpegQuality: 500));

        Assert.AreEqual(MagickFormat.Jpeg, image.Format);
        Assert.AreEqual(100u, image.Quality);
        Assert.AreEqual(8u, image.Depth);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Render_ChannelScales_ChangeOnlyThoseChannels()
    {
        using var image = Grey();

        MagickRawDeveloper.Render(image, new RawDevelopOptions(RedScale: 2, GreenScale: 1, BlueScale: 0.5));

        var pixel = image.GetPixels().GetPixel(0, 0).ToColor()!;
        Assert.AreEqual(200, pixel.R / 257);
        Assert.AreEqual(100, pixel.G / 257);
        Assert.AreEqual(50, pixel.B / 257);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Develop_NotRaw_InvalidData()
    {
        using var image = Grey();
        var png = image.ToByteArray(MagickFormat.Png);

        await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
            new MagickRawDeveloper().DevelopAsync(new MemoryStream(png), new MemoryStream(), new RawDevelopOptions(), CancellationToken.None));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Develop_BadBitDepth_NotSupported() =>
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() =>
            new MagickRawDeveloper().DevelopAsync(new MemoryStream(), new MemoryStream(), new RawDevelopOptions(BitDepth: 12), CancellationToken.None));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsRaw_Png_False()
    {
        using var image = Grey();

        Assert.IsFalse(new MagickRawDeveloper().IsRaw(new MemoryStream(image.ToByteArray(MagickFormat.Png))));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Registration_ResolvesDefaultAndKeyedDeveloper()
    {
        using var provider = new ServiceCollection().TryAddMagickRawDeveloper().BuildServiceProvider();

        Assert.IsInstanceOfType<MagickRawDeveloper>(provider.GetRequiredService<IRawDeveloper>());
        Assert.AreSame(provider.GetRequiredService<IRawDeveloper>(), provider.GetRequiredKeyedService<IRawDeveloper>(ImageMagickGlobals.ProviderKey));
    }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public async Task Develop_RealRawFile_ProducesJpeg()
    {
        var path = TestContext.GetPropertyOrDefault("RAW_TEST_FILE", string.Empty);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Assert.Inconclusive("Set RAW_TEST_FILE to a camera RAW file.");
        }

        using var input = File.OpenRead(path);
        using var output = new MemoryStream();

        await new MagickRawDeveloper().DevelopAsync(input, output, new RawDevelopOptions(RedScale: 1.1), CancellationToken.None);

        Assert.IsTrue(output.Length > 1000);
        Assert.AreEqual(0xFF, output.ToArray()[0]);
    }
}
