using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Ports;
using OoBDev.System.IO.Segmenters;
using System.ComponentModel;

namespace OoBDev.Fencing.ScoreMachines.Favero;

/// <summary>A Favero score machine. It sends a 10-byte frame (starting <c>FF</c>) unprompted at 2400 baud, 8N1; the device only receives.</summary>
[SerialPort(2400, Parity.None, 8, StopBits.One)]
[Description("Favero")]
public sealed class FaveroDevice : IDeviceDefinitionReceiver<IScoreMachineState>
{
    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; } =
        Segment.StartsWith(FaveroStateParser.Start)
               .AndIsLength(FaveroStateParser.FrameLength)
               .WithOptions(SegmentionOptions.SkipInvalidSegment | SegmentionOptions.SecondStartInvalid);

    /// <inheritdoc/>
    public IMessageDecoder<IScoreMachineState> Decoder { get; } = new FaveroMessageDecoder();
}
