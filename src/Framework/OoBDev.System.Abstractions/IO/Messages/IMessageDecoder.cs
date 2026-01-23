using System.Buffers;

namespace OoBDev.System.IO.Messages;

/// <summary>
/// Provides functionality for decoding byte sequences into typed messages.
/// </summary>
/// <typeparam name="TResponse">The type of message to decode to.</typeparam>
public interface IMessageDecoder<TResponse>
{
    /// <summary>
    /// Decodes a byte sequence into a typed message.
    /// </summary>
    /// <param name="response">The byte sequence to decode.</param>
    /// <returns>The decoded message.</returns>
    TResponse Decode(ReadOnlySequence<byte> response);
}
