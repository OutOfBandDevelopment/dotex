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

/// <summary>One field of a <see cref="FrameSchema"/>: where it sits is implied by the fields before it.</summary>
public sealed class FrameField
{
    private static readonly Dictionary<string, int> _fixedSizes = new(StringComparer.Ordinal)
    {
        ["u1"] = 1,
        ["s1"] = 1,
        ["u2"] = 2,
        ["s2"] = 2,
        ["u4"] = 4,
        ["s4"] = 4,
        ["f4"] = 4,
        ["u8"] = 8,
        ["s8"] = 8,
        ["f8"] = 8,
    };

    /// <summary>The published value id; unused for a <c>skip</c> field.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary><c>u1 u2 u4 u8 s1 s2 s4 s8 f4 f8</c> (numbers), <c>b1</c> to <c>b64</c> (a bit field, packed MSB first), <c>str</c> (ASCII text, trailing NULs and spaces dropped), <c>bytes</c> (published as hex) or <c>skip</c> (not published).</summary>
    public string Type { get; set; } = "u1";

    /// <summary><c>le</c> or <c>be</c> for this field only; the frame's endian when unset.</summary>
    public string? Endian { get; set; }

    /// <summary>Length in bytes, required for <c>str</c>, <c>bytes</c> and <c>skip</c>.</summary>
    public int? Size { get; set; }

    /// <summary>A number's raw value is multiplied by this (then <see cref="Offset"/> is added): a 16-bit value in tenths of a volt has Scale 0.1.</summary>
    public double? Scale { get; set; }

    public double? Offset { get; set; }

    public string? Unit { get; set; }

    /// <summary>What the picker and a generated panel call it; the name when unset.</summary>
    public string? Label { get; set; }

    /// <summary>A declared range (after scaling), used for sample data and a generated gauge.</summary>
    public double? Minimum { get; set; }

    public double? Maximum { get; set; }

    /// <summary>Hex bytes this field must equal (a magic number, a fixed command byte); a frame where it differs is discarded and the decoder re-syncs.</summary>
    public string? Expect { get; set; }

    [JsonIgnore]
    public bool Publishes => !string.Equals(Type, "skip", StringComparison.Ordinal);

    /// <summary>The width in bits when <see cref="Type"/> is a bit field (<c>b1</c> to <c>b64</c>, an unsigned value read MSB first), else 0.</summary>
    [JsonIgnore]
    public int BitWidth => Type.Length is > 1 and <= 3 && Type[0] == 'b' && int.TryParse(Type.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var width) && width is >= 1 and <= 64 ? width : 0;

    [JsonIgnore]
    public bool IsNumber => _fixedSizes.ContainsKey(Type) || BitWidth > 0;

    [JsonIgnore]
    public int? EffectiveSize => BitWidth > 0 ? null : _fixedSizes.TryGetValue(Type, out var size) ? size : Size is > 0 ? Size : null;

    internal static byte[] ParseHex(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var digits = text.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase);
        if (digits.Length % 2 != 0)
        {
            throw new FormatException("Hex needs an even number of digits.");
        }

        return Convert.FromHexString(digits);
    }

    internal IEnumerable<string> Validate(bool openTail = false)
    {
        var which = string.IsNullOrWhiteSpace(Name) ? $"A '{Type}' frame field" : $"Frame field '{Name}'";
        if (Publishes && string.IsNullOrWhiteSpace(Name))
        {
            yield return $"{which} needs a name.";
        }

        if (!IsNumber && Type is not ("str" or "bytes" or "skip"))
        {
            yield return $"{which} has an unknown type '{Type}' (u1 u2 u4 u8 s1 s2 s4 s8 f4 f8 b1..b64 str bytes skip).";
        }
        else if (!IsNumber && Size is not > 0 && !(openTail && Type is "bytes" or "str"))
        {
            yield return $"{which} is '{Type}' and needs a Size.";
        }

        if (BitWidth > 0 && Expect is { Length: > 0 })
        {
            yield return $"{which} is a bit field; Expect works on whole-byte fields only.";
        }

        if (Endian is not null && !string.Equals(Endian, "le", StringComparison.OrdinalIgnoreCase) && !string.Equals(Endian, "be", StringComparison.OrdinalIgnoreCase))
        {
            yield return $"{which} endian '{Endian}' must be 'le' or 'be'.";
        }

        if (Expect is { Length: > 0 })
        {
            byte[]? expected = null;
            string? bad = null;
            try
            {
                expected = ParseHex(Expect);
            }
            catch (FormatException)
            {
                bad = $"{which} Expect '{Expect}' isn't hex bytes.";
            }

            if (bad is not null)
            {
                yield return bad;
            }

            if (expected is not null && BitWidth == 0 && EffectiveSize is { } size && expected.Length != size)
            {
                yield return string.Create(CultureInfo.InvariantCulture, $"{which} Expect has {expected.Length} byte(s) but the field is {size}.");
            }
        }
    }
}
