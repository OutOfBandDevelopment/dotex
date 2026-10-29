using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.IO.Segmenters;
using OoBDev.TestUtilities;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class SegmenterTests
{
    private static ISegmenter Build(ISegmentBuildDefinition definition, List<byte[]> received) =>
        definition.ThenDo(segment =>
        {
            received.Add(segment.ToArray());
            return Task.CompletedTask;
        });

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task FixedLengthWithoutStartCutsExactPacketsTest()
    {
        var received = new List<byte[]>();
        var segmenter = Build(Segment.PassThough().AndIsLength(4), received);

        var result = await segmenter.TryReadAsync(new ReadOnlySequence<byte>([1, 2, 3, 4, 5, 6])).ConfigureAwait(false);

        Assert.AreEqual(SegmentationStatus.Complete, result.Status);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, received[0]);
        CollectionAssert.AreEqual(new byte[] { 5, 6 }, result.RemainingData.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task FixedLengthWithoutStartWaitsForMoreBytesTest()
    {
        var received = new List<byte[]>();
        var segmenter = Build(Segment.PassThough().AndIsLength(4), received);

        var result = await segmenter.TryReadAsync(new ReadOnlySequence<byte>([1, 2, 3])).ConfigureAwait(false);

        Assert.AreEqual(SegmentationStatus.Incomplete, result.Status);
        Assert.AreEqual(0, received.Count);
        Assert.AreEqual(3, result.RemainingData.Length);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task FixedLengthWithoutStartAcrossSplitBufferTest()
    {
        var received = new List<byte[]>();
        var segmenter = Build(Segment.PassThough().AndIsLength(4), received);
        var first = new Chunk([1, 2]);
        var last = first.Append([3, 4, 5]);

        var result = await segmenter.TryReadAsync(new ReadOnlySequence<byte>(first, 0, last, 3)).ConfigureAwait(false);

        Assert.AreEqual(SegmentationStatus.Complete, result.Status);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, received[0]);
        CollectionAssert.AreEqual(new byte[] { 5 }, result.RemainingData.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task LengthFieldCountsTheRestOfThePacketTest()
    {
        // Radex One shape: fixed 8-byte header, a 16-bit little-endian count of the extension bytes at offset 4.
        var received = new List<byte[]>();
        var segmenter = Build(Segment.StartsWith(0x7A).AndIsLength(8).ExtendedWithLengthAt<ushort>(4, Endianness.Little), received);
        byte[] packet = [0x7A, 0xFF, 0x20, 0x00, 0x03, 0x00, 0x00, 0x00, 0xAA, 0xBB, 0xCC];

        var result = await segmenter.TryReadAsync(new ReadOnlySequence<byte>([.. packet, 0x01])).ConfigureAwait(false);

        Assert.AreEqual(SegmentationStatus.Complete, result.Status);
        CollectionAssert.AreEqual(packet, received[0]);
        CollectionAssert.AreEqual(new byte[] { 0x01 }, result.RemainingData.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task LengthFieldWaitsForTheWholeExtensionTest()
    {
        var received = new List<byte[]>();
        var segmenter = Build(Segment.StartsWith(0x7A).AndIsLength(8).ExtendedWithLengthAt<ushort>(4, Endianness.Little), received);

        var result = await segmenter.TryReadAsync(new ReadOnlySequence<byte>([0x7A, 0xFF, 0x20, 0x00, 0x03, 0x00, 0x00, 0x00, 0xAA])).ConfigureAwait(false);

        Assert.AreEqual(SegmentationStatus.Incomplete, result.Status);
        Assert.AreEqual(0, received.Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task StartMarkerMatchesOneByteNotAPrefixSequenceTest()
    {
        // Documents the limit that api-design.md records: the start bytes are alternatives, not a sequence, so a
        // two-byte prefix such as 0x7A 0xFF cannot be required. A lone 0xFF is accepted as a start.
        var received = new List<byte[]>();
        var segmenter = Build(Segment.StartsWith(0x7A, 0xFF).AndIsLength(2), received);

        var result = await segmenter.TryReadAsync(new ReadOnlySequence<byte>([0x00, 0xFF, 0x11, 0x22])).ConfigureAwait(false);

        Assert.AreEqual(SegmentationStatus.Complete, result.Status);
        CollectionAssert.AreEqual(new byte[] { 0xFF, 0x11 }, received[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task UsingRunsACustomSegmenterFromTheBuilderTest()
    {
        var received = new List<byte[]>();
        var segmenter = Build(Segment.Using(on => new FixedLengthSegmenterStub(on)), received);

        var result = await segmenter.TryReadAsync(new ReadOnlySequence<byte>([0x01, 0x02, 0x03])).ConfigureAwait(false);

        Assert.AreEqual(SegmentationStatus.Complete, result.Status);
        CollectionAssert.AreEqual(new byte[] { 0x01 }, received[0]);
    }

    private sealed class FixedLengthSegmenterStub(OnSegmentReceived onSegmentReceived) : SegmenterBase(onSegmentReceived, SegmentionOptions.None)
    {
        protected override (SegmentationStatus status, ReadOnlySequence<byte>? segment) Read(ReadOnlySequence<byte> buffer) =>
            buffer.IsEmpty ? (SegmentationStatus.Incomplete, null) : (SegmentationStatus.Complete, buffer.Slice(0, 1));
    }

    private sealed class Chunk : ReadOnlySequenceSegment<byte>
    {
        public Chunk(byte[] data) => Memory = data;

        public Chunk Append(byte[] data)
        {
            var next = new Chunk(data) { RunningIndex = RunningIndex + Memory.Length };
            Next = next;
            return next;
        }
    }
}
