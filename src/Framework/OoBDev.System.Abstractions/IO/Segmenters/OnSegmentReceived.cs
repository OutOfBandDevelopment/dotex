using System.Buffers;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Segmenters;

public delegate Task OnSegmentReceived(ReadOnlySequence<byte> segment);
