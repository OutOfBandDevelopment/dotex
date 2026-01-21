using System;

namespace OoBDev.System.IO.Messages;

/// <summary>
/// Provides functionality for encoding typed messages into byte sequences.
/// </summary>
/// <typeparam name="TMessage">The type of message to encode.</typeparam>
public interface IMessageEncoder<TMessage>
{
    /// <summary>
    /// Encodes a typed message into a byte sequence.
    /// </summary>
    /// <param name="request">The message to encode.</param>
    /// <returns>A read-only byte sequence containing the encoded message.</returns>
    ReadOnlyMemory<byte> Encode(ref TMessage request);
}
