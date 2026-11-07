using System;
using System.Buffers;
using System.Collections.Generic;
using OoBDev.UiDefinitions;

namespace OoBDev.DerEE.De5000.Panels;

/// <summary>Cuts the received stream into 17-byte packets (dropping a byte at a time to resync, like <see cref="De5000Segmenter"/>), writes the readable line of each and publishes the values that changed since the last packet.</summary>
public sealed class De5000Decoder : IDeviceDecoder
{
    private readonly List<byte> _buffer = [];
    private readonly Dictionary<string, string> _last = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public string Name => DerEEGlobals.ProviderKey;

    /// <inheritdoc />
    public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged;

    /// <inheritdoc />
    public IReadOnlyList<string> Decode(ReadOnlySequence<byte> data)
    {
        foreach (var segment in data)
        {
            _buffer.AddRange(segment.Span.ToArray());
        }

        var lines = new List<string>();
        while (_buffer.Count >= De5000Frame.Length)
        {
            var packet = _buffer.GetRange(0, De5000Frame.Length).ToArray();
            if (!De5000Frame.TryParse(packet, out var frame))
            {
                _buffer.RemoveAt(0);
                continue;
            }

            _buffer.RemoveRange(0, De5000Frame.Length);
            var reading = De5000Interpreter.Interpret(frame);
            lines.Add(reading.Text);
            Publish(reading.Values);
        }

        return lines;
    }

    private void Publish(IReadOnlyDictionary<string, string> values)
    {
        Dictionary<string, string>? changed = null;
        foreach (var (id, value) in values)
        {
            if (!_last.TryGetValue(id, out var previous) || !string.Equals(previous, value, StringComparison.Ordinal))
            {
                (changed ??= [])[id] = value;
                _last[id] = value;
            }
        }

        if (changed is not null)
        {
            ValuesChanged?.Invoke(this, changed);
        }
    }
}
