using System;
using System.Runtime.InteropServices;

namespace OoBDev.System.IO.Messages;

/// <summary>
/// Provides message encoding functionality using marshalling to convert structured types to byte sequences.
/// </summary>
/// <typeparam name="TMessage">The type of message to encode, must be a blittable struct.</typeparam>
public class MessageEncoder<TMessage> : IMessageEncoder<TMessage>
{
    /// <summary>
    /// Encodes a typed message into a byte sequence using marshalling.
    /// </summary>
    /// <param name="request">The message to encode.</param>
    /// <returns>A read-only byte sequence containing the encoded message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
    public ReadOnlyMemory<byte> Encode(ref TMessage request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        var requestBuffer = new byte[Marshal.SizeOf(request)];
        var ptr = Marshal.AllocHGlobal(requestBuffer.Length);
        Marshal.StructureToPtr(request, ptr, true);
        Marshal.Copy(ptr, requestBuffer, 0, requestBuffer.Length);
        Marshal.FreeHGlobal(ptr);
        ReadOnlyMemory<byte> span = requestBuffer;
        return span;
    }
}
