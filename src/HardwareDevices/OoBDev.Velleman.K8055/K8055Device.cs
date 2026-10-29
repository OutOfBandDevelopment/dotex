using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;
using OoBDev.System.IO.UsbHids;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// A Velleman K8055 USB experiment board (vendor 0x10CF, product 0x5500 for the board at address 0). It streams its inputs without
/// being asked and takes <see cref="K8055Command"/> writes for its outputs and counters.
/// </summary>
[UsbHid(0x10CF, 0x5500)]
public sealed class K8055Device : IDeviceDefinitionReceiver<K8055Reading>, IDeviceDefinitionTransmitter<K8055Command>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } = Segment.PassThough().AndIsLength(K8055Reading.Length);

    /// <inheritdoc/>
    public IMessageDecoder<K8055Reading> Decoder { get; } = new K8055MessageDecoder();

    /// <inheritdoc/>
    public IMessageEncoder<K8055Command> Encoder { get; } = new K8055MessageEncoder();
}
