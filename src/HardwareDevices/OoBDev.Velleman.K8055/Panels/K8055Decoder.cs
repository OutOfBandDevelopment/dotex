using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using OoBDev.UiDefinitions;

namespace OoBDev.Velleman.K8055.Panels;

/// <summary>
/// Decodes the K8055's unprompted 9-byte input report <c>[00, digitalIn, 03, analogIn1, analogIn2, counter1Lo, counter1Hi, counter2Lo, counter2Hi]</c>
/// (byte 0 is the HID report id). Publishes the values under the ids <see cref="K8055UiDefinition"/> declares.
/// </summary>
/// <remarks>
/// The per-bit mapping of the digital-input byte is unconfirmed (nothing was wired to the input pins during the hardware check),
/// so it is published whole as <c>digitalInRaw</c> in hex.
/// </remarks>
public sealed class K8055Decoder : IDeviceDecoder
{
    private const int _frameLength = 9;

    private readonly List<byte> _buffer = [];
    private readonly Dictionary<string, string> _lastValues = [];

    /// <inheritdoc />
    public string Name => "k8055";

    /// <inheritdoc />
    public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged;

    /// <inheritdoc />
    public IReadOnlyList<string> Decode(ReadOnlySequence<byte> data)
    {
        foreach (var segment in data)
        {
            _buffer.AddRange(segment.Span);
        }

        var lines = new List<string>();
        while (_buffer.Count >= _frameLength)
        {
            var frame = _buffer.GetRange(0, _frameLength);
            _buffer.RemoveRange(0, _frameLength);
            lines.Add(DecodeFrame(frame, out var values));

            // The device repeats the same report continuously, so publish only what changed and nothing when nothing did.
            Dictionary<string, string>? changed = null;
            foreach (var (id, value) in values)
            {
                if (!_lastValues.TryGetValue(id, out var previous) || !string.Equals(previous, value, StringComparison.Ordinal))
                {
                    (changed ??= [])[id] = value;
                    _lastValues[id] = value;
                }
            }

            if (changed is not null)
            {
                ValuesChanged?.Invoke(this, changed);
            }
        }

        return lines;
    }

    private static string DecodeFrame(List<byte> frame, out IReadOnlyDictionary<string, string> values)
    {
        var digitalInRaw = frame[1];
        var analogIn1 = frame[3];
        var analogIn2 = frame[4];
        var counter1 = (ushort)(frame[5] | (frame[6] << 8));
        var counter2 = (ushort)(frame[7] | (frame[8] << 8));

        var digitalInRawText = $"0x{digitalInRaw:X2}";
        var analogIn1Text = analogIn1.ToString(CultureInfo.InvariantCulture);
        var analogIn2Text = analogIn2.ToString(CultureInfo.InvariantCulture);
        var counter1Text = counter1.ToString(CultureInfo.InvariantCulture);
        var counter2Text = counter2.ToString(CultureInfo.InvariantCulture);

        values = new Dictionary<string, string>
        {
            ["digitalInRaw"] = digitalInRawText,
            ["analogIn1"] = analogIn1Text,
            ["analogIn2"] = analogIn2Text,
            ["counter1"] = counter1Text,
            ["counter2"] = counter2Text,
        };

        return $"IN: D={digitalInRawText} A1={analogIn1Text} A2={analogIn2Text} C1={counter1Text} C2={counter2Text}";
    }
}
