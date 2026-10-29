using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;

namespace OoBDev.Nmea;

/// <summary>
/// A receiver that sends NMEA 0183 sentences without being asked, on a serial port, TCP or USB HID. The definition does not choose the
/// transport; <see cref="NmeaEarthmateBt20Device"/> adds the USB HID identity of one receiver.
/// </summary>
public class NmeaDevice : IDeviceDefinitionReceiver<NmeaSentence>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } = Segment.Using(on => new NmeaSegmenter(on));

    /// <inheritdoc/>
    public IMessageDecoder<NmeaSentence> Decoder { get; } = new NmeaMessageDecoder();
}
