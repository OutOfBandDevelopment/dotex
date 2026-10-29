using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// The shared definition of a K8055 board. The USB product id depends on the board address set by the SK5 and SK6 jumpers, so each
/// address has its own sealed class carrying the <c>[UsbHid]</c> attribute.
/// </summary>
public abstract class K8055DeviceBase : IDeviceDefinitionReceiver<K8055Reading>, IDeviceDefinitionTransmitter<K8055Command>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } = Segment.PassThough().AndIsLength(K8055Reading.Length);

    /// <inheritdoc/>
    public IMessageDecoder<K8055Reading> Decoder { get; } = new K8055MessageDecoder();

    /// <inheritdoc/>
    public IMessageEncoder<K8055Command> Encoder { get; } = new K8055MessageEncoder();
}
