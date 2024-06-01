using System.Buffers;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Segmenters;

public interface ISegmenter
{
    ValueTask<ISegmentReadResult> TryReadAsync(ReadOnlySequence<byte> buffer);
}