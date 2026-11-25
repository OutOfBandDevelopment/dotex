using System;
using System.Buffers;
using System.Collections.Generic;
using OoBDev.UiDefinitions;

namespace OoBDev.Kuando.Busylight.Panels;

/// <summary>
/// Decodes the Busylight's poll reply, an ASCII device-identification string (for example <c>0001PLENOM0000010000000 1DASAN...</c>,
/// confirmed on hardware) rather than per-field telemetry, so it publishes no live values.
/// </summary>
public sealed class BusylightDecoder : IDeviceDecoder
{
    /// <inheritdoc />
    public string Name => "busylight";

    /// <inheritdoc />
    public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Decode(ReadOnlySequence<byte> data)
    {
        if (data.IsEmpty)
        {
            return [];
        }

        var text = new string(Array.ConvertAll(data.ToArray(), b => b is >= 0x20 and < 0x7F ? (char)b : '.'));
        return [$"BUSYLIGHT: {text}"];
    }
}
