using System;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// A segment definition backed by a device-specific <see cref="ISegmenter"/>, for streams the fluent builder cannot describe
/// (for example Z-Wave, where lone ACK, NAK and CAN bytes are interleaved with length-prefixed frames).
/// </summary>
/// <param name="factory">Creates the segmenter that reports each segment to the supplied callback.</param>
internal sealed class CustomSegmentBuildDefinition(Func<OnSegmentReceived, ISegmenter> factory) : ISegmentBuildDefinition
{
    internal ISegmenter Create(OnSegmentReceived onSegmentReceived) => factory(onSegmentReceived);
}
