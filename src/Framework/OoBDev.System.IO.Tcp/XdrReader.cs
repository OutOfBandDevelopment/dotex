using System;
using System.Buffers.Binary;
using System.IO;

namespace OoBDev.System.IO.Tcp;

/// <summary>Reads XDR items; throws <see cref="InvalidDataException"/> on a short or oversized buffer instead of reading past the end.</summary>
internal sealed class XdrReader(byte[] buffer)
{
    private int _offset;

    public int Remaining => buffer.Length - _offset;

    public uint UInt32()
    {
        if (Remaining < 4)
        {
            throw new InvalidDataException("The RPC reply is shorter than expected.");
        }

        var value = BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan(_offset, 4));
        _offset += 4;
        return value;
    }

    public byte[] Opaque()
    {
        var length = UInt32();
        if (length > int.MaxValue || length + XdrWriter.Padding((int)length) > Remaining)
        {
            throw new InvalidDataException("The RPC reply carries more data than it holds.");
        }

        var data = buffer.AsSpan(_offset, (int)length).ToArray();
        _offset += (int)length + XdrWriter.Padding((int)length);
        return data;
    }
}
