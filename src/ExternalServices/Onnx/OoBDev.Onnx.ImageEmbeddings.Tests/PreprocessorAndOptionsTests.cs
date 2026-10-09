using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Linq;

namespace OoBDev.Onnx.ImageEmbeddings.Tests;

[TestClass]
public class PreprocessorAndOptionsTests
{
    private static DecodedImage Solid(int width, int height, byte r, byte g, byte b)
    {
        var rgb = new byte[width * height * 3];
        for (var i = 0; i < rgb.Length; i += 3)
        {
            rgb[i] = r;
            rgb[i + 1] = g;
            rgb[i + 2] = b;
        }
        return new DecodedImage(width, height, rgb);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Process_Solid_WritesChannelPlanesNormalised()
    {
        var options = new OnnxImageEmbeddingOptions { ImageSize = 4, Mean = [0.5f, 0.25f, 0f], Std = [0.5f, 0.25f, 1f] };
        var preprocessor = new ImagePreprocessor(options);
        var destination = new float[preprocessor.Length];

        preprocessor.Process(Solid(9, 5, 255, 0, 51), destination);

        Assert.AreEqual(3 * 4 * 4, destination.Length);
        Assert.IsTrue(destination.Take(16).All(v => Math.Abs(v - 1f) < 1e-5), "red plane");
        Assert.IsTrue(destination.Skip(16).Take(16).All(v => Math.Abs(v + 1f) < 1e-5), "green plane");
        Assert.IsTrue(destination.Skip(32).All(v => Math.Abs(v - 0.2f) < 1e-5), "blue plane");
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Process_ShortestEdgeAndCrop_TakesCenterOfResizedImage()
    {
        // 8x4 image: left half black, right half white. Shorter edge 4 keeps 8x4; crop 4 takes columns 2..5 (both halves).
        var rgb = new byte[8 * 4 * 3];
        for (var y = 0; y < 4; y++)
            for (var x = 4; x < 8; x++)
                for (var c = 0; c < 3; c++) rgb[(((y * 8) + x) * 3) + c] = 255;
        var options = new OnnxImageEmbeddingOptions
        {
            ImageSize = 4,
            ResizeShortestEdge = 4,
            CenterCrop = true,
            Resample = ImageResample.Bilinear,
            Mean = [0f, 0f, 0f],
            Std = [1f, 1f, 1f],
        };
        var preprocessor = new ImagePreprocessor(options);
        var destination = new float[preprocessor.Length];

        preprocessor.Process(new DecodedImage(8, 4, rgb), destination);

        var row = destination.Take(4).ToArray();
        CollectionAssert.AreEqual(new[] { 0f, 0f, 1f, 1f }, row);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Options_Defaults_AreValid() => new OnnxImageEmbeddingOptions().Validate();

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Options_OutOfRange_Throw()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new OnnxImageEmbeddingOptions { ImageSize = 0 }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new OnnxImageEmbeddingOptions { Mean = [0f] }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new OnnxImageEmbeddingOptions { Std = [1f, 0f, 1f] }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new OnnxImageEmbeddingOptions { MaxBatchSize = 0 }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new OnnxImageEmbeddingOptions { MaxConcurrentInferences = 0 }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new OnnxImageEmbeddingOptions { MaxPixels = 0 }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new OnnxImageEmbeddingOptions { CenterCrop = true }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new OnnxImageEmbeddingOptions { CenterCrop = true, ResizeShortestEdge = 100 }.Validate());
    }
}
