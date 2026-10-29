using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;
using OoBDev.System.IO.UsbHids;

namespace OoBDev.Kuando.Busylight;

/// <summary>
/// A Kuando Busylight status light (USB HID, vendor 0x04D8, product 0xF848). Commands go out as <see cref="BusylightCommand"/>;
/// the only thing it sends back is the identification reply to a poll.
/// </summary>
[UsbHid(0x04D8, 0xF848)]
public sealed class BusylightDevice : IDeviceDefinitionReceiver<BusylightIdentification>, IDeviceDefinitionTransmitter<BusylightCommand>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } = Segment.Using(on => new BusylightSegmenter(on));

    /// <inheritdoc/>
    public IMessageDecoder<BusylightIdentification> Decoder { get; } = new BusylightMessageDecoder();

    /// <inheritdoc/>
    public IMessageEncoder<BusylightCommand> Encoder { get; } = new BusylightMessageEncoder();
}
