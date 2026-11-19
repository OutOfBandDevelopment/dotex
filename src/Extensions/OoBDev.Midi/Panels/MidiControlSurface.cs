using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.Midi.Panels;

/// <summary>Turns panel commands into MIDI bytes. The sliders only set the channel, note, velocity and program; the buttons send.</summary>
public sealed class MidiControlSurface : IControlSurface, ICommandPreview
{
    private readonly DeviceSend _send;
    private int _channel = 1, _note = 60, _velocity = 100, _program;

    /// <summary>Creates a surface that writes through <paramref name="send"/>.</summary>
    /// <param name="send">The device write.</param>
    public MidiControlSurface(DeviceSend send)
    {
        ArgumentNullException.ThrowIfNull(send);
        _send = send;
    }

    /// <inheritdoc />
    public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        if (Setting(commandId, value))
        {
            return Task.CompletedTask;
        }

        var bytes = Bytes(commandId) ?? throw new ArgumentException($"Unknown MIDI command '{commandId}'.", nameof(commandId));
        return _send(bytes, cancellationToken);
    }

    /// <inheritdoc />
    public string? PreviewCommand(string commandId, string? value)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        return Bytes(commandId) is { } bytes ? CommandPreviewFormat.ToHex(bytes) : null;
    }

    private bool Setting(string commandId, string? value)
    {
        ref var target = ref _channel;
        int max;
        int min = 0;
        switch (commandId)
        {
            case "channel":
                target = ref _channel;
                min = 1;
                max = 16;
                break;
            case "note":
                target = ref _note;
                max = 127;
                break;
            case "velocity":
                target = ref _velocity;
                max = 127;
                break;
            case "program":
                target = ref _program;
                max = 127;
                break;
            default:
                return false;
        }

        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            throw new ArgumentException($"'{commandId}' needs a number.", nameof(value));
        }

        target = Math.Clamp((int)Math.Round(number), min, max);
        return true;
    }

    private byte[]? Bytes(string commandId)
    {
        var channel = (byte)(_channel - 1);
        return commandId switch
        {
            "noteOn" => [(byte)(0x90 | channel), (byte)_note, (byte)_velocity],
            "noteOff" => [(byte)(0x80 | channel), (byte)_note, 0],
            "programChange" => [(byte)(0xC0 | channel), (byte)_program],
            "allNotesOff" => [(byte)(0xB0 | channel), 123, 0],
            "allSoundOff" => [(byte)(0xB0 | channel), 120, 0],
            "start" => [0xFA],
            "continue" => [0xFB],
            "stop" => [0xFC],
            "reset" => [0xFF],
            _ => null,
        };
    }
}
