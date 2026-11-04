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

/// <summary>
/// A fixed-layout binary frame a device sends: an optional sync prefix, then fields at consecutive offsets. Each field
/// publishes a live value under its <see cref="FrameField.Name"/> (dotted names like <c>header.length</c> are fine), the
/// binary counterpart to <see cref="InboundProtocol.Patterns"/>' regex captures. The <c>.ksy</c> importer
/// (<see cref="KsyImporter"/>) generates one of these from a Kaitai Struct file. See docs/design/UiDefinitions/ksy-importer.md.
/// </summary>
public sealed class FrameSchema
{
    /// <summary>Hex bytes (<c>"A5 5A"</c>) every frame starts with; the decoder finds frames by them and re-syncs after garbage. Unset means frames are back to back from the first byte.</summary>
    public string? Sync { get; set; }

    /// <summary><c>le</c> (the default) or <c>be</c>: the byte order of multi-byte numbers a field doesn't override.</summary>
    public string Endian { get; set; } = "le";

    public List<FrameField> Fields { get; set; } = [];

    /// <summary>
    /// Makes the frame variable-length: the name of a numeric field (<c>u1 u2 u4</c> or a bit field) holding a length. The
    /// last field must then be a <c>bytes</c> or <c>str</c> with no <see cref="FrameField.Size"/>; it takes whatever is left
    /// of the frame length (this field's value plus <see cref="LengthAdjust"/>) after the fixed fields and any checksum.
    /// </summary>
    public string? LengthField { get; set; }

    /// <summary>Added to the <see cref="LengthField"/> value to get the whole frame's length in bytes: 0 when it already counts everything, or the number of bytes it doesn't count (a length of the payload alone needs the header size here).</summary>
    public int LengthAdjust { get; set; }

    /// <summary>A checksum or CRC the frame ends with; a frame where it doesn't match is discarded.</summary>
    public FrameChecksum? Checksum { get; set; }

    /// <summary>The whole frame's length in bytes, or null when it varies (<see cref="IsVariable"/>) or a field's size is unknown (<see cref="Validate"/> reports why).</summary>
    [JsonIgnore]
    public int? Length => IsVariable ? null : Layout()?.MinLength;

    /// <summary>True when the last field takes its size from <see cref="LengthField"/>.</summary>
    [JsonIgnore]
    public bool IsVariable => Layout() is { TailIndex: >= 0 };

    /// <summary>The shortest frame in bytes (the whole length for a fixed frame), or null when a field's size is unknown.</summary>
    [JsonIgnore]
    public int? MinLength => Layout()?.MinLength;

    /// <summary>Where each field sits: bit fields pack MSB first, a byte-sized field starts on the next byte boundary. Null when a field's size is unknown.</summary>
    internal FrameLayout? Layout()
    {
        var slots = new List<FrameSlot>(Fields.Count);
        var cursor = 0;
        var tail = -1;
        for (var i = 0; i < Fields.Count; i++)
        {
            var field = Fields[i];
            if (field.BitWidth > 0)
            {
                slots.Add(new FrameSlot(field, cursor, field.BitWidth));
                cursor += field.BitWidth;
                continue;
            }

            cursor = (cursor + 7) / 8 * 8;
            if (field.EffectiveSize is { } size)
            {
                slots.Add(new FrameSlot(field, cursor, size * 8));
                cursor += size * 8;
            }
            else if (i == Fields.Count - 1 && field.Type is "bytes" or "str" && !string.IsNullOrWhiteSpace(LengthField))
            {
                tail = i;
                slots.Add(new FrameSlot(field, cursor, 0));
            }
            else
            {
                return null;
            }
        }

        var checksumBytes = Checksum is { } checksum && FrameChecksum.SizeOf(checksum.Kind) is { } checksumSize ? checksumSize : 0;
        return new FrameLayout(slots, (cursor + 7) / 8, tail, checksumBytes);
    }

    /// <summary>The sync prefix as bytes, or empty. Throws <see cref="FormatException"/> for text that isn't hex.</summary>
    public byte[] SyncBytes() => FrameField.ParseHex(Sync);

    /// <summary>What is wrong with this schema: empty when it can decode frames.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (!string.Equals(Endian, "le", StringComparison.OrdinalIgnoreCase) && !string.Equals(Endian, "be", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"Frame endian '{Endian}' must be 'le' or 'be'.");
        }

        try
        {
            SyncBytes();
        }
        catch (FormatException)
        {
            errors.Add($"Frame sync '{Sync}' isn't hex bytes (for example \"A5 5A\").");
        }

        if (Fields.Count == 0)
        {
            errors.Add("The frame has no fields.");
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < Fields.Count; i++)
        {
            var field = Fields[i];
            errors.AddRange(field.Validate(openTail: i == Fields.Count - 1 && !string.IsNullOrWhiteSpace(LengthField)));
            if (field.Publishes && !string.IsNullOrWhiteSpace(field.Name) && !names.Add(field.Name))
            {
                errors.Add($"Frame field '{field.Name}' appears more than once.");
            }
        }

        if (!string.IsNullOrWhiteSpace(LengthField))
        {
            var target = Fields.FirstOrDefault(f => string.Equals(f.Name, LengthField, StringComparison.Ordinal));
            if (target is null || !target.IsNumber || target.Type is "f4" or "f8")
            {
                errors.Add($"Frame LengthField '{LengthField}' must name an integer field (u1 u2 u4 u8 s1 s2 s4 s8 or a bit field).");
            }
            else if (Fields.Count > 0 && ReferenceEquals(target, Fields[^1]))
            {
                errors.Add($"Frame LengthField '{LengthField}' must come before the variable-size last field.");
            }

            if (Fields.Count > 0 && Fields[^1] is { IsNumber: false } last && last.Type is not ("bytes" or "str"))
            {
                errors.Add("A variable-length frame must end with a 'bytes' or 'str' field with no Size.");
            }
        }

        if (Checksum is { } checksum)
        {
            errors.AddRange(checksum.Validate());
        }

        if (errors.Count == 0 && Checksum is { } sum && Layout() is { } layout && sum.Start > layout.FixedBytes)
        {
            errors.Add($"Frame checksum Start {sum.Start} is past the end of the fixed fields.");
        }

        return errors;
    }
}
