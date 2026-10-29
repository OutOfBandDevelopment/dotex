using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;

namespace OoBDev.DerEE.De5000;

/// <summary>
/// A DE-5000 LCR meter. It transmits a packet continuously and takes no commands, so the device only receives. The bytes come from
/// the optical-to-USB-serial or optical-to-BLE adapter through the existing adapters.
/// </summary>
public sealed class De5000Device : IDeviceDefinitionReceiver<De5000Frame>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } = Segment.Using(on => new De5000Segmenter(on));

    /// <inheritdoc/>
    public IMessageDecoder<De5000Frame> Decoder { get; } = new De5000MessageDecoder();
}
