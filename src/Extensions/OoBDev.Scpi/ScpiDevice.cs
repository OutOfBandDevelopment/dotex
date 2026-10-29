using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;
using System;

namespace OoBDev.Scpi;

/// <summary>
/// An instrument that answers text commands, on a serial port, TCP or USB. The profile decides how replies are cut: a profile with a
/// terminator gets line segmentation, and one without (Korad, whose replies have no terminator) treats each arrival as one reply.
/// </summary>
public sealed class ScpiDevice : IDeviceDefinitionReceiver<ScpiReply>, IDeviceDefinitionTransmitter<ScpiRequest>
{
    /// <summary>Creates a device for <paramref name="profile"/>.</summary>
    /// <param name="profile">The instrument profile.</param>
    public ScpiDevice(ScpiInstrumentProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        Profile = profile;
        SegmentDefintion = profile.Terminator.Length == 0 ? Segment.PassThough() : Segment.Using(on => new ScpiReplySegmenter(on));
    }

    /// <summary>The instrument profile.</summary>
    public ScpiInstrumentProfile Profile { get; }

    /// <inheritdoc/>
    public ISegmentBuildDefinition SegmentDefintion { get; }

    /// <inheritdoc/>
    public IMessageDecoder<ScpiReply> Decoder { get; } = new ScpiMessageDecoder();

    /// <inheritdoc/>
    public IMessageEncoder<ScpiRequest> Encoder { get; } = new ScpiMessageEncoder();
}
