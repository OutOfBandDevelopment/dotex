using System;

namespace OoBDev.System.IO.Messages;

public interface IMessageEncoder<TMessage>
{
    ReadOnlyMemory<byte> Encode(ref TMessage request);
}
