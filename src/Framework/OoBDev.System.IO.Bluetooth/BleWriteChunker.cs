using System;
using System.Collections.Generic;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>
/// Splits an outgoing BLE write into successive chunks no larger than <paramref name="maxChunkSize"/> —
/// a without-response GATT write is limited to one ATT packet (negotiated MTU minus the 3-byte ATT
/// header), so a buffer larger than that either fails or gets silently truncated by the stack on
/// common NUS/HM-10-class peripherals.
/// </summary>
public static class BleWriteChunker
{
    public static IEnumerable<ReadOnlyMemory<byte>> Chunk(ReadOnlyMemory<byte> data, int maxChunkSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxChunkSize, 0);

        for (var offset = 0; offset < data.Length; offset += maxChunkSize)
        {
            yield return data.Slice(offset, global::System.Math.Min(maxChunkSize, data.Length - offset));
        }
    }
}
