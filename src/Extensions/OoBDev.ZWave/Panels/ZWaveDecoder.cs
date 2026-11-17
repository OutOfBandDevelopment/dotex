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

    /// <summary>Creates the decoder over a node table.</summary>
    /// <param name="table">The table the decoded frames are folded into, or <see langword="null"/> for a new one.</param>
    public ZWaveDecoder(ZWaveNodeTable? table = null) => Table = table ?? new ZWaveNodeTable();

    /// <summary>Gets what is known about the network, built from the frames this decoder has seen. The control surface of the same session shares it.</summary>
    public ZWaveNodeTable Table { get; }

    /// <summary>Occurs for every unit decoded, before its description is published.</summary>
    public event EventHandler<ZWaveFrame>? FrameDecoded;

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
        FrameDecoded?.Invoke(this, frame);
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

                if (Table.Apply(frame) is not null)
                {
                    values["nodes"] = string.Join(Environment.NewLine, Table.Describe());
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
