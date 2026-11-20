using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Documents.Hpgl.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class VectorSceneAudioWriterTests
{
    private static VectorScene Square() => new(100, 100,
    [
        new VectorPolyline(1, "#000000", [new(10, 10), new(90, 10), new(90, 90), new(10, 90), new(10, 10)]),
    ]);

    private static VectorSceneAudioOptions Short() => new() { Duration = TimeSpan.FromMilliseconds(100) };

    [TestMethod]
    public void Write_Square_ProducesStereoPcmWavOfTheRequestedLength()
    {
        using var ms = new MemoryStream();
        new VectorSceneAudioWriter(Short()).Write(Square(), ms);
        var b = ms.ToArray();

        Assert.AreEqual("RIFF", global::System.Text.Encoding.ASCII.GetString(b, 0, 4));
        Assert.AreEqual("WAVE", global::System.Text.Encoding.ASCII.GetString(b, 8, 4));
        Assert.AreEqual(2, BitConverter.ToInt16(b, 22));
        Assert.AreEqual(48000, BitConverter.ToInt32(b, 24));
        Assert.AreEqual(4800 * 4, BitConverter.ToInt32(b, 40));
        Assert.AreEqual(44 + (4800 * 4), b.Length);
    }

    [TestMethod]
    public void Write_Square_SamplesStayInRangeAndSpreadOverBothChannels()
    {
        using var ms = new MemoryStream();
        new VectorSceneAudioWriter(Short()).Write(Square(), ms);
        var b = ms.ToArray();

        short minX = short.MaxValue, maxX = short.MinValue, minY = short.MaxValue, maxY = short.MinValue;
        for (var i = 44; i < b.Length; i += 4)
        {
            var x = BitConverter.ToInt16(b, i);
            var y = BitConverter.ToInt16(b, i + 2);
            minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
        }

        Assert.IsTrue(maxX - minX > 20000);
        Assert.IsTrue(maxY - minY > 20000);
    }

    [TestMethod]
    public void Write_EmptyScene_WritesNothing()
    {
        using var ms = new MemoryStream();
        new VectorSceneAudioWriter(Short()).Write(VectorScene.Empty, ms);

        Assert.AreEqual(0, ms.Length);
    }

    [TestMethod]
    public void Write_TwoLines_InsertsJumpSamplesBetweenThem()
    {
        var scene = new VectorScene(100, 100,
        [
            new VectorPolyline(1, "#000000", [new(0, 0), new(10, 0)]),
            new VectorPolyline(1, "#000000", [new(90, 90), new(100, 90)]),
        ]);
        using var ms = new MemoryStream();
        new VectorSceneAudioWriter(new VectorSceneAudioOptions { Duration = TimeSpan.FromMilliseconds(50), JumpSamples = 6 }).Write(scene, ms);

        Assert.IsTrue(ms.Length > 44);
    }

    [TestMethod]
    public void Constructor_InvalidOptions_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new VectorSceneAudioWriter(new VectorSceneAudioOptions { SampleRate = 0 }));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new VectorSceneAudioWriter(new VectorSceneAudioOptions { Duration = TimeSpan.Zero }));
        Assert.ThrowsExactly<ArgumentNullException>(() => new VectorSceneAudioWriter(null!));
    }
}
