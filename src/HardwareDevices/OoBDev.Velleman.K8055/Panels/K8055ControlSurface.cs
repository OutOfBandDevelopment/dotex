using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.Velleman.K8055.Panels;

/// <summary>
/// <see cref="IControlSurface"/> for the Velleman K8055. The "set outputs" (0x05) frame carries the digital-out bitmask and
/// both analog-out bytes together, so every output change resends the full frame with the last-known state of the rest.
/// Every frame starts with the <c>0x00</c> HID report-id byte (confirmed on hardware); bytes 6-7 (duration/debounce) are sent as zero.
/// </summary>
public sealed class K8055ControlSurface : IControlSurface, ICommandPreview
{
    private const byte _setOutputsCommand = 0x05;
    private const byte _resetCounter1Command = 0x03;
    private const byte _resetCounter2Command = 0x04;

    private readonly DeviceSend _send;
    private readonly Lock _stateLock = new();
    private (byte DigitalOut, byte AnalogOut1, byte AnalogOut2) _state;

    /// <summary>Creates a surface that writes its frames through <paramref name="send"/>.</summary>
    /// <param name="send">The device write.</param>
    public K8055ControlSurface(DeviceSend send)
    {
        ArgumentNullException.ThrowIfNull(send);
        _send = send;
    }

    /// <inheritdoc />
    public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandId);

        byte[] frame;
        lock (_stateLock)
        {
            (frame, _state) = Plan(commandId, value, _state);
        }

        return _send(frame, cancellationToken);
    }

    /// <summary>The report <see cref="InvokeAsync"/> would send, as hex bytes, without changing any output state; null for an unknown command or unparsable value.</summary>
    public string? PreviewCommand(string commandId, string? value)
    {
        ArgumentNullException.ThrowIfNull(commandId);

        try
        {
            byte[] frame;
            lock (_stateLock)
            {
                (frame, _) = Plan(commandId, value, _state);
            }

            return CommandPreviewFormat.ToHex(frame);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
        {
            return null;
        }
    }

    private static (byte[] Frame, (byte DigitalOut, byte AnalogOut1, byte AnalogOut2) NewState) Plan(
        string commandId,
        string? value,
        (byte DigitalOut, byte AnalogOut1, byte AnalogOut2) state)
    {
        if (TryParseDigitalOutChannel(commandId, out var channelIndex))
        {
            var mask = (byte)(1 << channelIndex);
            var digitalOut = ParseBool(value) ? (byte)(state.DigitalOut | mask) : (byte)(state.DigitalOut & ~mask);
            var newState = state with { DigitalOut = digitalOut };
            return (BuildSetOutputsFrame(newState), newState);
        }

        switch (commandId)
        {
            case "analogOut1":
                {
                    var newState = state with { AnalogOut1 = ParseByte(value) };
                    return (BuildSetOutputsFrame(newState), newState);
                }

            case "analogOut2":
                {
                    var newState = state with { AnalogOut2 = ParseByte(value) };
                    return (BuildSetOutputsFrame(newState), newState);
                }

            case "resetCounter1":
                return (BuildFixedFrame(_resetCounter1Command), state);
            case "resetCounter2":
                return (BuildFixedFrame(_resetCounter2Command), state);
            default:
                throw new ArgumentException($"Unknown K8055 command '{commandId}'.", nameof(commandId));
        }
    }

    private static bool TryParseDigitalOutChannel(string commandId, out int channelIndex)
    {
        channelIndex = -1;
        const string Prefix = "digitalOut";
        if (!commandId.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(commandId.AsSpan(Prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var channel) || channel is < 1 or > 8)
        {
            return false;
        }

        channelIndex = channel - 1;
        return true;
    }

    private static bool ParseBool(string? value) => value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private static byte ParseByte(string? value)
    {
        var number = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
        return (byte)Math.Clamp(number, 0, 255);
    }

    private static byte[] BuildSetOutputsFrame((byte DigitalOut, byte AnalogOut1, byte AnalogOut2) state) =>
        [0x00, _setOutputsCommand, state.DigitalOut, state.AnalogOut1, state.AnalogOut2, 0x00, 0x00, 0x00, 0x00];

    private static byte[] BuildFixedFrame(byte command) => [0x00, command, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
}
