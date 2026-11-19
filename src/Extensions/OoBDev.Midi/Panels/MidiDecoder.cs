using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using OoBDev.UiDefinitions;

namespace OoBDev.Midi.Panels;

/// <summary>Decodes the incoming MIDI stream with <see cref="MidiStreamDecoder"/> and shows each message as a line. Timing clock and active sensing are dropped, because they arrive many times a second and would bury everything else.</summary>
public sealed class MidiDecoder : IDeviceDecoder
{
    private readonly MidiStreamDecoder _decoder = new();

    /// <inheritdoc />
    public string Name => "midi";

    /// <inheritdoc />
    public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged;

    /// <inheritdoc />
    public IReadOnlyList<string> Decode(ReadOnlySequence<byte> data)
    {
        var lines = new List<string>();
        foreach (var message in _decoder.Feed(data.ToArray()))
        {
            if (message.Status is 0xF8 or 0xFE)
            {
                continue;
            }

            var text = Describe(message);
            lines.Add(text);
            ValuesChanged?.Invoke(this, new Dictionary<string, string>(StringComparer.Ordinal) { ["last"] = text });
        }

        return lines;
    }

    /// <summary>Describes a message in words.</summary>
    /// <param name="m">The message.</param>
    /// <returns>A one-line description.</returns>
    public static string Describe(MidiMessage m)
    {
        ArgumentNullException.ThrowIfNull(m);
        var c = (m.Channel + 1).ToString(CultureInfo.InvariantCulture);
        return m.Type switch
        {
            0x8 => $"Note off ch{c} {m.Data1}",
            0x9 when m.Data2 == 0 => $"Note off ch{c} {m.Data1}",
            0x9 => $"Note on ch{c} {m.Data1} vel {m.Data2}",
            0xA => $"Aftertouch ch{c} {m.Data1} {m.Data2}",
            0xB => $"Controller ch{c} {m.Data1} = {m.Data2}",
            0xC => $"Program ch{c} {m.Data1}",
            0xD => $"Pressure ch{c} {m.Data1}",
            0xE => $"Pitch bend ch{c} {m.Data1 | (m.Data2 << 7)}",
            0xF0 => $"SysEx {m.SysEx?.Length ?? 0} bytes",
            0xFA => "Start",
            0xFB => "Continue",
            0xFC => "Stop",
            0xFF => "Reset",
            _ => $"Status 0x{m.Status:X2}",
        };
    }
}
