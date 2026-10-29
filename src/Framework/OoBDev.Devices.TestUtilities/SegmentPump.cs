using OoBDev.System.IO;
using OoBDev.System.IO.Segmenters;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Devices.TestUtilities;

/// <summary>
/// Feeds received bytes through a device definition's segmenter and decoder the way the stream pipeline does, collecting the
/// decoded messages, so a test needs neither a port nor a running <c>StreamDevice</c>.
/// </summary>
/// <typeparam name="TMessage">The device's message type.</typeparam>
public sealed class SegmentPump<TMessage>
{
    private readonly ISegmenter _segmenter;
    private byte[] _buffer = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentPump{TMessage}"/> class.
    /// </summary>
    /// <param name="device">The device definition whose receive side is exercised.</param>
    public SegmentPump(IDeviceDefinitionReceiver<TMessage> device)
    {
        ArgumentNullException.ThrowIfNull(device);
        _segmenter = device.SegmentDefintion.ThenAs(device.Decoder, message =>
        {
            Messages.Add(message);
            return Task.CompletedTask;
        });
    }

    /// <summary>Gets the messages decoded so far, in arrival order.</summary>
    public IList<TMessage> Messages { get; } = [];

    /// <summary>Gets the bytes buffered that do not yet form a whole segment.</summary>
    public ReadOnlyMemory<byte> Pending => _buffer;

    /// <summary>
    /// Adds received bytes and decodes every segment they complete.
    /// </summary>
    /// <param name="received">The bytes just read from the device.</param>
    /// <returns>A task that completes when no further segment can be cut.</returns>
    public async Task PushAsync(ReadOnlyMemory<byte> received)
    {
        _buffer = [.. _buffer, .. received.Span];
        while (_buffer.Length > 0)
        {
            var result = await _segmenter.TryReadAsync(new ReadOnlySequence<byte>(_buffer)).ConfigureAwait(false);
            if (result.Status != SegmentationStatus.Complete) return;
            _buffer = result.RemainingData.ToArray();
        }
    }
}
