using System;
using System.Buffers;

namespace OoBDev.Scpi;

/// <summary>
/// The <c>#&lt;n&gt;&lt;length&gt;</c> prefix of an IEEE 488.2 definite-length block: <c>n</c> is one digit (1 to 9) giving
/// the number of digits in <c>length</c>, which is the byte count of the data that follows.
/// </summary>
internal static class ScpiBlockHeader
{
    internal enum Result
    {
        NotABlock,
        Incomplete,
        Block,
    }

    internal static Result TryRead(ReadOnlySequence<byte> buffer, out int headerLength, out int dataLength)
    {
        headerLength = 0;
        dataLength = 0;
        if (buffer.IsEmpty || buffer.FirstSpan[0] != (byte)'#') return Result.NotABlock;
        if (buffer.Length < 2) return Result.Incomplete;

        Span<byte> prefix = stackalloc byte[11];
        var available = (int)global::System.Math.Min(buffer.Length, prefix.Length);
        buffer.Slice(0, available).CopyTo(prefix);

        var digits = prefix[1] - (byte)'0';
        if (digits is < 1 or > 9) return Result.NotABlock;
        if (available < 2 + digits) return Result.Incomplete;

        long length = 0;
        for (var i = 0; i < digits; i++)
        {
            var d = prefix[2 + i] - (byte)'0';
            if (d is < 0 or > 9) return Result.NotABlock;
            length = (length * 10) + d;
        }
        if (length > int.MaxValue) return Result.NotABlock;

        headerLength = 2 + digits;
        dataLength = (int)length;
        return Result.Block;
    }
}
