using System.Buffers;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Represents an asynchronous callback that is invoked when a data segment is received.
/// </summary>
/// <param name="segment">The received byte segment.</param>
/// <returns>A task representing the asynchronous segment handling operation.</returns>
public delegate Task OnSegmentReceived(ReadOnlySequence<byte> segment);
