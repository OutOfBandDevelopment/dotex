using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;

namespace OoBDev.Zoom.H4n;

/// <summary>
/// A Zoom H4n recorder on its RC04 / RC2 remote port, tapped as a serial port (2400 baud, 8-N-1, no handshake) by the h4n2rs485 adapter.
/// Every received byte is a complete <see cref="ZoomH4nStatus"/>.
/// </summary>
public sealed class ZoomH4nDevice : IDeviceDefinitionReceiver<ZoomH4nStatus>, IDeviceDefinitionTransmitter<ZoomH4nCommand>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } = Segment.PassThough().AndIsLength(1);

    /// <inheritdoc/>
    public IMessageDecoder<ZoomH4nStatus> Decoder { get; } = new ZoomH4nMessageDecoder();

    /// <inheritdoc/>
    public IMessageEncoder<ZoomH4nCommand> Encoder { get; } = new ZoomH4nMessageEncoder();
}
