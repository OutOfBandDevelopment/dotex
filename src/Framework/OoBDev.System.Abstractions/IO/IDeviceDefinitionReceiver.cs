using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;

namespace OoBDev.System.IO;

/// <summary>
/// Represents a device definition for receiving messages of type <typeparamref name="TMessage"/>.
/// </summary>
/// <typeparam name="TMessage">The type of messages handled by the device.</typeparam>
public interface IDeviceDefinitionReceiver<TMessage> : IDeviceDefinition<TMessage>
{
    /// <summary>
    /// Gets the segment build definition associated with the device definition.
    /// </summary>
    ISegmentBuildDefinition SegmentDefintion { get; }

    /// <summary>
    /// Gets the message decoder used for decoding messages received by the device.
    /// </summary>
    IMessageDecoder<TMessage> Decoder { get; }
}
