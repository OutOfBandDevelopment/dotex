using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>A checksum the frame ends with. It covers the bytes from <see cref="Start"/> (0 is the first sync byte) up to the checksum itself.</summary>
public sealed class FrameChecksum
{
    private static readonly string[] _kinds = ["sum8", "xor8", "crc8", "crc16-modbus", "crc16-ccitt"];

    /// <summary><c>sum8</c> (low byte of the sum), <c>xor8</c>, <c>crc8</c> (poly 0x07), <c>crc16-modbus</c> (reflected 0xA001, init 0xFFFF; low byte first on the wire) or <c>crc16-ccitt</c> (CCITT-FALSE, poly 0x1021, init 0xFFFF; high byte first).</summary>
    public string Kind { get; set; } = "sum8";

    /// <summary>Bytes of the frame to skip before the covered data starts: 0 covers the sync bytes too, the sync's length starts just after them.</summary>
    public int Start { get; set; }

    /// <summary><c>le</c> or <c>be</c> for a 16-bit checksum's byte order on the wire; the kind's usual order when unset.</summary>
    public string? Endian { get; set; }

    /// <summary>The checksum's size in bytes, or null for an unknown kind.</summary>
    public static int? SizeOf(string kind) => kind switch
    {
        "sum8" or "xor8" or "crc8" => 1,
        "crc16-modbus" or "crc16-ccitt" => 2,
        _ => null,
    };

    internal IEnumerable<string> Validate()
    {
        if (SizeOf(Kind) is null)
        {
            yield return $"Frame checksum kind '{Kind}' must be one of {string.Join(", ", _kinds)}.";
        }

        if (Start < 0)
        {
            yield return "Frame checksum Start can't be negative.";
        }

        if (Endian is not null && !string.Equals(Endian, "le", StringComparison.OrdinalIgnoreCase) && !string.Equals(Endian, "be", StringComparison.OrdinalIgnoreCase))
        {
            yield return $"Frame checksum endian '{Endian}' must be 'le' or 'be'.";
        }
    }

    /// <summary>True when the frame's trailing checksum matches the covered bytes. <paramref name="frame"/> is the whole frame.</summary>
    internal bool Matches(ReadOnlySpan<byte> frame)
    {
        var size = SizeOf(Kind) ?? 0;
        if (size == 0 || frame.Length < size + Start)
        {
            return false;
        }

        var covered = frame[Start..^size];
        var trailer = frame[^size..];
        var computed = Compute(Kind, covered);
        if (size == 1)
        {
            return trailer[0] == (byte)computed;
        }

        var big = Endian is { } e ? string.Equals(e, "be", StringComparison.OrdinalIgnoreCase) : Kind == "crc16-ccitt";
        var onWire = big ? (ushort)((trailer[0] << 8) | trailer[1]) : (ushort)((trailer[1] << 8) | trailer[0]);
        return onWire == (ushort)computed;
    }

    internal static uint Compute(string kind, ReadOnlySpan<byte> data)
    {
        switch (kind)
        {
            case "sum8":
                var sum = 0;
                foreach (var b in data)
                {
                    sum += b;
                }

                return (uint)(sum & 0xFF);
            case "xor8":
                var x = 0;
                foreach (var b in data)
                {
                    x ^= b;
                }

                return (uint)x;
            case "crc8":
                var crc8 = 0;
                foreach (var b in data)
                {
                    crc8 ^= b;
                    for (var i = 0; i < 8; i++)
                    {
                        crc8 = (crc8 & 0x80) != 0 ? ((crc8 << 1) ^ 0x07) & 0xFF : (crc8 << 1) & 0xFF;
                    }
                }

                return (uint)crc8;
            case "crc16-modbus":
                var modbus = 0xFFFF;
                foreach (var b in data)
                {
                    modbus ^= b;
                    for (var i = 0; i < 8; i++)
                    {
                        modbus = (modbus & 1) != 0 ? (modbus >> 1) ^ 0xA001 : modbus >> 1;
                    }
                }

                return (uint)modbus;
            case "crc16-ccitt":
                var ccitt = 0xFFFF;
                foreach (var b in data)
                {
                    ccitt ^= b << 8;
                    for (var i = 0; i < 8; i++)
                    {
                        ccitt = (ccitt & 0x8000) != 0 ? ((ccitt << 1) ^ 0x1021) & 0xFFFF : (ccitt << 1) & 0xFFFF;
                    }
                }

                return (uint)ccitt;
            default:
                return 0;
        }
    }
}
