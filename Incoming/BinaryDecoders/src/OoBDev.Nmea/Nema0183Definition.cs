using OoBDev.IO;
using OoBDev.IO.Messages;
using OoBDev.IO.Ports;
using OoBDev.IO.Segmenters;
using OoBDev.IO.UsbHids;
using System.ComponentModel;
using System.Composition;
using static OoBDev.IO.Bytes;

namespace OoBDev.Nmea;

[SerialPort(4800)]
[UsbHid(0x1163, 0x200)]
[Description("NEMA 0183")]
[Export(typeof(IDeviceDefinition))]
public class Nema0183Definition : IDeviceDefinitionReceiver<INema0183Message>
{
    public ISegmentBuildDefinition SegmentDefintion => Segment.StartsWith("$!"u8.ToArray()).AndEndsWith(Lf);
    public IMessageDecoder<INema0183Message> Decoder { get; } = new Nema0183Decoder();
}
