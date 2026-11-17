using System.Collections.Generic;

namespace OoBDev.Midi;

/// <summary>
/// Decodes the MIDI wire protocol from a byte stream (serial or USB) into messages. Handles running status, real-time bytes
/// interleaved with other messages, and system exclusive. Stateful: bytes may arrive in any chunking. Data bytes with no status
/// to attach to are dropped, and a system exclusive longer than <see cref="MaxSysExLength"/> is abandoned.
/// </summary>
public sealed class MidiStreamDecoder
{
    /// <summary>The most bytes kept for one system exclusive message.</summary>
    public const int MaxSysExLength = 65536;

    private byte _running;
    private byte _status;
    private int _needed;
    private byte _first;
    private int _have;
    private List<byte>? _sysEx;

    /// <summary>
    /// Feeds bytes and returns the messages they completed.
    /// </summary>
    /// <param name="bytes">The next bytes of the stream.</param>
    /// <returns>The completed messages in order; empty when none completed.</returns>
    public IReadOnlyList<MidiMessage> Feed(IEnumerable<byte> bytes)
    {
        var messages = new List<MidiMessage>();
        foreach (var b in bytes)
        {
            if (b >= 0xF8)
            {
                messages.Add(new MidiMessage(b, 0, 0));
            }
            else if (b >= 0x80)
            {
                Status(b, messages);
            }
            else
            {
                Data(b, messages);
            }
        }

        return messages;
    }

    private void Status(byte b, List<MidiMessage> messages)
    {
        if (_sysEx is not null && b != 0xF7)
        {
            _sysEx = null;
        }

        _have = 0;
        switch (b)
        {
            case 0xF0:
                _sysEx = [];
                _running = 0;
                _status = 0;
                _needed = 0;
                break;
            case 0xF7:
                if (_sysEx is not null)
                {
                    messages.Add(new MidiMessage(0xF0, 0, 0, [.. _sysEx]));
                    _sysEx = null;
                }

                _status = 0;
                _needed = 0;
                break;
            case >= 0xF4:
                _running = 0;
                _status = 0;
                _needed = 0;
                messages.Add(new MidiMessage(b, 0, 0));
                break;
            default:
                _status = b;
                _needed = b is 0xF1 or 0xF3 || (b is >= 0x80 and < 0xF0 && (b >> 4) is 0xC or 0xD) ? 1 : 2;
                _running = b < 0xF0 ? b : (byte)0;
                break;
        }
    }

    private void Data(byte b, List<MidiMessage> messages)
    {
        if (_sysEx is not null)
        {
            if (_sysEx.Count < MaxSysExLength)
            {
                _sysEx.Add(b);
            }
            else
            {
                _sysEx = null;
            }

            return;
        }

        if (_status == 0 && _running != 0)
        {
            _status = _running;
            _needed = (_running >> 4) is 0xC or 0xD ? 1 : 2;
            _have = 0;
        }

        if (_status == 0)
        {
            return;
        }

        if (_have == 0)
        {
            _first = b;
        }

        _have++;
        if (_have < _needed)
        {
            return;
        }

        messages.Add(new MidiMessage(_status, _first, _needed == 2 ? b : (byte)0));
        _have = 0;
        _status = _running;
        _needed = _running == 0 ? 0 : (_running >> 4) is 0xC or 0xD ? 1 : 2;
    }
}
