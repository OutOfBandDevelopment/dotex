using OoBDev.System.IO.Messages;

namespace OoBDev.System.IO;

/// <summary>
/// Represents a device definition for transmitting messages of type <typeparamref name="TMessage"/>.
/// </summary>
/// <typeparam name="TMessage">The type of messages handled by the device.</typeparam>
public interface IDeviceDefinitionTransmitter<TMessage> : IDeviceDefinition<TMessage>
{
    /// <summary>
    /// Gets the message encoder used for encoding messages to be transmitted by the device.
    /// </summary>
    IMessageEncoder<TMessage> Encoder { get; }
}
