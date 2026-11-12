using System;
using System.Buffers;
using System.Collections.Generic;
using OoBDev.UiDefinitions;

namespace OoBDev.ZWave.Panels;

/// <summary>Cuts the incoming Serial API byte stream into frames and describes each one; the latest description feeds the panel's status.</summary>
public sealed class ZWaveDecoder : IDeviceDecoder
{
    private readonly ZWaveMessageDecoder _frames = new();
    private readonly List<byte> _pending = [];

    /// <inheritdoc/>
    public string Name => ZWaveGlobals.ProviderKey;

    /// <inheritdoc/>
    public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged;

    /// <inheritdoc/>
    public IReadOnlyList<string> Decode(ReadOnlySequence<byte> data)
    {
        _pending.AddRange(data.ToArray());
        var lines = new List<string>();
        while (_pending.Count > 0)
        {
            var length = NextUnitLength();
            if (length == 0)
            {
                break;
            }

            var raw = _pending.GetRange(0, length).ToArray();
            _pending.RemoveRange(0, length);
            Describe(_frames.Decode(new ReadOnlySequence<byte>(raw)), lines);
        }

        return lines;
    }

    // 0 means the unit is not complete yet.
    private int NextUnitLength()
    {
        if (_pending[0] != ZWaveFrame.StartOfFrame)
        {
            return 1;
        }

        if (_pending.Count < 2)
        {
            return 0;
        }

        var length = _pending[1];
        if (length < 3)
        {
            return 1;
        }

        return _pending.Count < length + 2 ? 0 : length + 2;
    }

    private void Describe(ZWaveFrame frame, List<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        string text;
        switch (frame.Kind)
        {
            case ZWaveFrameKind.Ack:
                text = "ACK";
                break;
            case ZWaveFrameKind.Nak:
                text = "NAK";
                break;
            case ZWaveFrameKind.Can:
                text = "CAN";
                break;
            case ZWaveFrameKind.Data:
                var reading = ZWaveInterpreter.Interpret(frame);
                text = reading.Text;
                foreach (var pair in reading.Values)
                {
                    values[pair.Key] = pair.Value;
                }

                break;
            default:
                text = $"INVALID: {ZWaveInterpreter.Hex(frame.Raw)}";
                break;
        }

        lines.Add(text);
        values["last"] = text;
        ValuesChanged?.Invoke(this, values);
    }
}
