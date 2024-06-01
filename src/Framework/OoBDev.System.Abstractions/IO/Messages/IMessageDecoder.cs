using System.Buffers;

namespace OoBDev.System.IO.Messages;

public interface IMessageDecoder<TResponse>
{
    TResponse Decode(ReadOnlySequence<byte> response);
}
