using OoBDev.System.IO.Messages;

namespace OoBDev.System.IO;

public interface IDeviceDefinitionTransmitter<TMessage> : IDeviceDefinition<TMessage>
{
    IMessageEncoder<TMessage> Encoder { get; }
}
