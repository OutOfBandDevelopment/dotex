using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;

namespace OoBDev.System.IO;

public interface IDeviceDefinitionReceiver<TMessage> : IDeviceDefinition<TMessage>
{
    ISegmentBuildDefinition SegmentDefintion { get; }
    IMessageDecoder<TMessage> Decoder { get; }
}
