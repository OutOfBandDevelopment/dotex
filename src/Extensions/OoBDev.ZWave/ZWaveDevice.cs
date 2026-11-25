using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Ports;
using OoBDev.System.IO.Segmenters;

namespace OoBDev.ZWave;

/// <summary>
/// A Z-Wave controller (USB stick or HAT) on its Serial API, 115200 baud 8N1.
/// </summary>
[SerialPort(115200)]
public sealed class ZWaveDevice : IDeviceDefinitionReceiver<ZWaveFrame>, IDeviceDefinitionTransmitter<ZWaveFrame>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } = Segment.Using(on => new ZWaveSegmenter(on));

    /// <inheritdoc/>
    public IMessageDecoder<ZWaveFrame> Decoder { get; } = new ZWaveMessageDecoder();

    /// <inheritdoc/>
    public IMessageEncoder<ZWaveFrame> Encoder { get; } = new ZWaveMessageEncoder();
}
