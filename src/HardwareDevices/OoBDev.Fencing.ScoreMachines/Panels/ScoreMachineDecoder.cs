using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using OoBDev.Fencing.ScoreMachines.Favero;
using OoBDev.Fencing.ScoreMachines.SaintGeorge;
using OoBDev.UiDefinitions;

namespace OoBDev.Fencing.ScoreMachines.Panels;

/// <summary>
/// Cuts the serial stream into score machine frames (Favero: ten bytes after <c>FF</c>; Saint George: <c>SOH</c> to <c>EOT</c>, at most
/// 100 bytes), parses each and returns one line per frame. Partial frames are buffered; noise before a frame start is dropped.
/// </summary>
public sealed class ScoreMachineDecoder : IDeviceDecoder
{
    private const byte _soh = 0x01;
    private const byte _eot = 0x04;
    private const int _sgMaxLength = 100;

    private readonly MachineType _type;
    private readonly IParseScoreMachineState _parser;
    private readonly List<byte> _buffer = [];
    private readonly Dictionary<string, string> _shown = new(StringComparer.Ordinal);

    /// <summary>Creates a decoder for one machine family.</summary>
    /// <param name="type">The machine family.</param>
    public ScoreMachineDecoder(MachineType type)
    {
        _type = type;
        _parser = type == MachineType.Favero ? new FaveroStateParser() : new SgStateParser();
    }

    /// <inheritdoc />
    public string Name => _type == MachineType.Favero ? FencingGlobals.FaveroKey : FencingGlobals.SaintGeorgeKey;

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
                if (_type == MachineType.Favero)
                {
                    AddFavero(value, lines);
                }
                else
                {
                    AddSaintGeorge(value, lines);
                }
            }
        }

        return lines;
    }

    private void AddFavero(byte value, List<string> lines)
    {
        if (_buffer.Count == 0 && value != FaveroStateParser.Start)
        {
            return;
        }

        _buffer.Add(value);
        if (_buffer.Count == FaveroStateParser.FrameLength)
        {
            Complete(lines);
        }
    }

    private void AddSaintGeorge(byte value, List<string> lines)
    {
        if (value == _soh)
        {
            _buffer.Clear();
        }
        else if (_buffer.Count == 0)
        {
            return;
        }

        _buffer.Add(value);
        if (value == _eot)
        {
            Complete(lines);
        }
        else if (_buffer.Count >= _sgMaxLength)
        {
            _buffer.Clear();
        }
    }

    private void Complete(List<string> lines)
    {
        var frame = _buffer.ToArray();
        _buffer.Clear();
        IScoreMachineState? state;
        try
        {
            state = _parser.Parse(frame);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or IndexOutOfRangeException or InvalidOperationException)
        {
            lines.Add($"BAD FRAME: {Convert.ToHexString(frame)}");
            return;
        }

        if (state is null)
        {
            return;
        }

        lines.Add(state.ToString()!);
        Publish(state);
    }

    private void Publish(IScoreMachineState state)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["clock"] = state.Clock.ToString(@"mm\:ss", CultureInfo.InvariantCulture),
            ["match"] = state.Match.ToString(CultureInfo.InvariantCulture),
        };
        Side(values, "left", state.Left);
        Side(values, "right", state.Right);

        var changed = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            if (!_shown.TryGetValue(pair.Key, out var old) || !string.Equals(old, pair.Value, StringComparison.Ordinal))
            {
                _shown[pair.Key] = pair.Value;
                changed[pair.Key] = pair.Value;
            }
        }

        if (changed.Count > 0)
        {
            ValuesChanged?.Invoke(this, changed);
        }
    }

    private static void Side(Dictionary<string, string> values, string prefix, Fencer fencer)
    {
        values[$"{prefix}Score"] = fencer.Score.ToString(CultureInfo.InvariantCulture);
        values[$"{prefix}Lights"] = fencer.Lights.ToString();
        values[$"{prefix}Cards"] = fencer.Cards.ToString();
        values[$"{prefix}Priority"] = fencer.Priority ? "yes" : "no";
    }
}
