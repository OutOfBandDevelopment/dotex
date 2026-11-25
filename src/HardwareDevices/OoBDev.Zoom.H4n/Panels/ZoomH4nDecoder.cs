using System;
using System.Buffers;
using System.Collections.Generic;
using OoBDev.UiDefinitions;

namespace OoBDev.Zoom.H4n.Panels;

/// <summary>Decodes the recorder's unprompted status bytes (<see cref="ZoomH4nStatus"/>, each a complete snapshot) into one text line per byte and publishes the five status indicators of <see cref="ZoomH4nUiDefinition"/> when one changes. The handshake wake byte (high bit) is a snapshot too: its documented bits are decoded and the rest ignored.</summary>
public sealed class ZoomH4nDecoder : IDeviceDecoder
{
    private const byte _documentedBits = 0x73;

    private readonly Dictionary<string, string> _last = [];

    /// <inheritdoc />
    public string Name => "zoom-h4n";

    /// <inheritdoc />
    public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged;

    /// <inheritdoc />
    public IReadOnlyList<string> Decode(ReadOnlySequence<byte> data)
    {
        var lines = new List<string>();
        foreach (var segment in data)
        {
            foreach (var value in segment.Span)
            {
                var status = (ZoomH4nStatus)(value & _documentedBits);
                lines.Add($"STATUS: 0x{value:X2} {(status == ZoomH4nStatus.None ? "none" : status.ToString())}");
                Publish(status);
            }
        }

        return lines;
    }

    private void Publish(ZoomH4nStatus status)
    {
        var current = new (string Id, bool On)[]
        {
            ("statusRecord", status.HasFlag(ZoomH4nStatus.Record)),
            ("statusPeak", status.HasFlag(ZoomH4nStatus.Peak)),
            ("statusMic", status.HasFlag(ZoomH4nStatus.Mic)),
            ("statusLed1", status.HasFlag(ZoomH4nStatus.Led1)),
            ("statusLed2", status.HasFlag(ZoomH4nStatus.Led2)),
        };

        Dictionary<string, string>? changed = null;
        foreach (var (id, on) in current)
        {
            var text = on ? "On" : "Off";
            if (!_last.TryGetValue(id, out var previous) || !string.Equals(previous, text, StringComparison.Ordinal))
            {
                (changed ??= [])[id] = text;
                _last[id] = text;
            }
        }

        if (changed is not null)
        {
            ValuesChanged?.Invoke(this, changed);
        }
    }
}
