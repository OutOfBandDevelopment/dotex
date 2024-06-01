using System.Buffers;

namespace OoBDev.System.IO.Segmenters;

public interface ISegmentReadResult
{
    SegmentationStatus Status { get; }
    ReadOnlySequence<byte> RemainingData { get; }
}