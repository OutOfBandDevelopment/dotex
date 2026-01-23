using OoBDev.ElectronicScoringMachines.Fencing.Common;
using OoBDev.IO;
using OoBDev.IO.Messages;
using OoBDev.IO.Ports;
using OoBDev.IO.Segmenters;
using System.ComponentModel;
using System.Composition;

namespace OoBDev.ElectronicScoringMachines.Fencing.SaintGeorge;

[SerialPort(9600, Parity.None, 8, StopBits.One)]
[Description("Saint George")]
[Export(typeof(IDeviceDefinition))]
public class SgStateDefinition : IDeviceDefinitionReceiver<IScoreMachineState>
{
    public ISegmentBuildDefinition SegmentDefintion { get; } =
        Segment.StartsWith(ControlCharacters.StartOfHeading)
               .AndEndsWith(ControlCharacters.EndOfTransmission)
               .WithMaxLength(100)
               .WithOptions(SegmentionOptions.SkipInvalidSegment | SegmentionOptions.SecondStartInvalid);

    public IMessageDecoder<IScoreMachineState> Decoder { get; } = new SgStateDecoder();
}