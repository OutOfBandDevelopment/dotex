using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Bluetooth.Tests;

/// <summary>
/// A without-response GATT write is limited to
/// one ATT packet, so a write larger than the negotiated MTU must be split before it reaches the
/// characteristic.
/// </summary>
[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Ble)]
public sealed class BleWriteChunkerTests
{
    [TestCategory(TestCategories.BugRegression)]
    [TestMethod]
    public void Chunk_DataLargerThanTheLimit_SplitsIntoChunksNoLargerThanTheLimit()
    {
        var data = Enumerable.Range(0, 45).Select(i => (byte)i).ToArray();

        var chunks = BleWriteChunker.Chunk(data, maxChunkSize: 20).Select(c => c.ToArray()).ToList();

        Assert.AreEqual(3, chunks.Count);
        Assert.AreSequenceEqual(data[..20], chunks[0]);
        Assert.AreSequenceEqual(data[20..40], chunks[1]);
        Assert.AreSequenceEqual(data[40..45], chunks[2]);
    }

    [TestMethod]
    public void Chunk_DataAtOrUnderTheLimit_YieldsOneChunkUnchanged()
    {
        var data = new byte[] { 1, 2, 3 };

        var chunks = BleWriteChunker.Chunk(data, maxChunkSize: 20).Select(c => c.ToArray()).ToList();

        Assert.AreEqual(1, chunks.Count);
        Assert.AreSequenceEqual(data, chunks[0]);
    }

    [TestMethod]
    public void Chunk_EmptyData_YieldsNoChunks()
    {
        Assert.IsEmpty(BleWriteChunker.Chunk(ReadOnlyMemory<byte>.Empty, maxChunkSize: 20));
    }

    [TestMethod]
    public void Chunk_NonPositiveLimit_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => BleWriteChunker.Chunk(new byte[] { 1 }, maxChunkSize: 0).ToList());
    }
}
