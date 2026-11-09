using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.System.IO.Tcp;

/// <summary>Writes XDR (RFC 4506) items: 4-byte big-endian integers and opaque/string data padded to a multiple of four.</summary>
internal sealed class XdrWriter
{
    private readonly List<byte> _bytes = [];

    public XdrWriter UInt32(uint value)
    {
        var buffer = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        _bytes.AddRange(buffer);
        return this;
    }

    public XdrWriter Bool(bool value) => UInt32(value ? 1u : 0u);

    public XdrWriter Opaque(ReadOnlySpan<byte> data)
    {
        UInt32((uint)data.Length);
        _bytes.AddRange(data.ToArray());
        for (var i = 0; i < Padding(data.Length); i++)
        {
            _bytes.Add(0);
        }

        return this;
    }

    public XdrWriter String(string value) => Opaque(Encoding.ASCII.GetBytes(value));

    public byte[] ToArray() => [.. _bytes];

    internal static int Padding(int length) => (4 - (length % 4)) % 4;
}
