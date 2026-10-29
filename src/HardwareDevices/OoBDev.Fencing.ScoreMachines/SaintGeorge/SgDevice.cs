using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Ports;
using OoBDev.System.IO.Segmenters;
using System.ComponentModel;

namespace OoBDev.Fencing.ScoreMachines.SaintGeorge;

/// <summary>A Saint George score machine. It sends frames from <c>SOH</c> to <c>EOT</c> unprompted at 9600 baud, 8N1; the device only receives.</summary>
[SerialPort(9600, Parity.None, 8, StopBits.One)]
[Description("Saint George")]
public sealed class SgDevice : IDeviceDefinitionReceiver<IScoreMachineState>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } =
        Segment.StartsWith(ControlCharacters.StartOfHeading)
               .AndEndsWith(ControlCharacters.EndOfTransmission)
               .WithMaxLength(100)
               .WithOptions(SegmentionOptions.SkipInvalidSegment | SegmentionOptions.SecondStartInvalid);

    /// <inheritdoc/>
    public IMessageDecoder<IScoreMachineState> Decoder { get; } = new SgMessageDecoder();
}
