using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using OoBDev.UiDefinitions;

namespace OoBDev.LanC.Panels;

/// <summary>Reads the bridge's answer: one text line per received LANC frame, dash-separated hex bytes (<c>18-33-02-...</c>) whose third byte is a <see cref="LanCStatusCode"/>. Lines end at a line feed; a carriage return is ignored. The line ending is unverified against the bridge.</summary>
public sealed class LanCDecoder : IDeviceDecoder
{
    private readonly StringBuilder _line = new();
    private string? _lastStatus;

    /// <inheritdoc />
    public string Name => "lanc";

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
                if (value == (byte)'\n')
                {
                    Complete(lines);
                }
                else if (value != (byte)'\r')
                {
                    _line.Append((char)value);
                }
            }
        }

        return lines;
    }

    private void Complete(List<string> lines)
    {
        var text = _line.ToString().Trim();
        _line.Clear();
        if (text.Length == 0)
        {
            return;
        }

        var parts = text.Split('-');
        var bytes = new byte[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!byte.TryParse(parts[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
            {
                lines.Add($"LINE: {text}");
                return;
            }
        }

        if (bytes.Length < 3)
        {
            lines.Add($"FRAME: {text}");
            return;
        }

        var status = Enum.IsDefined((LanCStatusCode)bytes[2]) ? ((LanCStatusCode)bytes[2]).ToString() : $"0x{bytes[2]:X2}";
        lines.Add($"STATUS: {status} ({text})");
        var values = new Dictionary<string, string>(StringComparer.Ordinal) { ["frame"] = text };
        if (!string.Equals(status, _lastStatus, StringComparison.Ordinal))
        {
            _lastStatus = status;
            values["status"] = status;
        }

        ValuesChanged?.Invoke(this, values);
    }
}
