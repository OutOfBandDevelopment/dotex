using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;

namespace OoBDev.HpglPlayer.Cli.Tests;

[TestClass]
public sealed class LoopingWaveProviderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public void Read_PastTheEndOfTheSource_RewindsAndKeepsFillingTheBuffer()
    {
        // Stage: four bytes of source, a ten byte request
        using var source = new RawSourceWaveStream(new MemoryStream([1, 2, 3, 4]), new WaveFormat(48000, 16, 2));
        var provider = new LoopingWaveProvider(source);
        var buffer = new byte[10];

        // Test
        var read = provider.Read(buffer, 0, buffer.Length);
        TestContext.WriteLine($"read={read} bytes={string.Join(",", buffer)}");

        // Assert
        Assert.AreEqual(10, read);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2 }, buffer);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Read_EmptySource_ReturnsZeroInsteadOfSpinning()
    {
        // Stage
        using var source = new RawSourceWaveStream(new MemoryStream([]), new WaveFormat(48000, 16, 2));
        var provider = new LoopingWaveProvider(source);

        // Test
        var read = provider.Read(new byte[8], 0, 8);

        // Assert
        Assert.AreEqual(0, read);
    }
}
