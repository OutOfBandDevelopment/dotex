using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// A Quarta Radex One dosimeter on a serial port (9600 baud, 8-N-1, no handshake). It answers each <see cref="RadexOneRequest"/> with
/// one <see cref="RadexOneReply"/>.
/// </summary>
public sealed class RadexOneDevice : IDeviceDefinitionReceiver<RadexOneReply>, IDeviceDefinitionTransmitter<RadexOneRequest>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } = Segment.Using(on => new RadexOneSegmenter(on));

    /// <inheritdoc/>
    public IMessageDecoder<RadexOneReply> Decoder { get; } = new RadexOneMessageDecoder();

    /// <inheritdoc/>
    public IMessageEncoder<RadexOneRequest> Encoder { get; } = new RadexOneMessageEncoder();
}
