using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OoBDev.EcoWitt;

/// <summary>
/// Decodes the response to the local API live data command (<see cref="EcoWittLocalApi.LiveDataCommand"/>). Pure: no I/O. The
/// data is a run of fields, each a one-byte id followed by a value whose size the id fixes. An id this decoder does not know
/// has no known size, so decoding stops there and the id is reported; the values before it are kept.
/// </summary>
public sealed class EcoWittLiveDataDecoder
{
    private const int _maxProblems = 8;

    private static readonly Dictionary<byte, Field> _fields = new()
    {
        [0x01] = new(2, 10, true, (r, v) => r with { IndoorTemperatureC = v }),
        [0x02] = new(2, 10, true, (r, v) => r with { OutdoorTemperatureC = v }),
        [0x03] = new(2, 10, true, (r, v) => r with { DewPointC = v }),
        [0x04] = new(2, 10, true, (r, v) => r with { WindChillC = v }),
        [0x05] = new(2, 10, true, (r, v) => r with { HeatIndexC = v }),
        [0x06] = new(1, 1, false, (r, v) => r with { IndoorHumidity = v }),
        [0x07] = new(1, 1, false, (r, v) => r with { OutdoorHumidity = v }),
        [0x08] = new(2, 10, false, (r, v) => r with { AbsolutePressureHpa = v }),
        [0x09] = new(2, 10, false, (r, v) => r with { RelativePressureHpa = v }),
        [0x0A] = new(2, 1, false, (r, v) => r with { WindDirectionDegrees = v }),
        [0x0B] = new(2, 10, false, (r, v) => r with { WindSpeedMs = v }),
        [0x0C] = new(2, 10, false, (r, v) => r with { WindGustMs = v }),
        [0x0D] = new(2, 10, false, (r, v) => r with { RainEventMm = v }),
        [0x0E] = new(2, 10, false, (r, v) => r with { RainRateMmPerHour = v }),
        [0x0F] = new(2, 10, false, (r, v) => r with { RainHourMm = v }),
        [0x10] = new(2, 10, false, (r, v) => r with { RainDayMm = v }),
        [0x11] = new(2, 10, false, (r, v) => r with { RainWeekMm = v }),
        [0x12] = new(4, 10, false, (r, v) => r with { RainMonthMm = v }),
        [0x13] = new(4, 10, false, (r, v) => r with { RainYearMm = v }),
        [0x15] = new(4, 10, false, (r, v) => r with { LightLux = v }),
        [0x16] = new(2, 10, false, (r, v) => r with { UvMicrowattPerCm2 = v }),
        [0x17] = new(1, 1, false, (r, v) => r with { UvIndex = v }),
    };

    /// <summary>
    /// Decodes one complete live data response, header to checksum.
    /// </summary>
    /// <param name="packet">The response bytes.</param>
    /// <returns>The reading, with <see cref="EcoWittLiveReading.Problems"/> set when a field was unknown or cut short.</returns>
    /// <exception cref="InvalidDataException">The framing is wrong (header, size, checksum) or the response answers another command.</exception>
    public EcoWittLiveReading Decode(ReadOnlySpan<byte> packet)
    {
        var data = EcoWittLocalApi.ReadPayload(packet, out var command);
        if (command != EcoWittLocalApi.LiveDataCommand)
        {
            throw new InvalidDataException($"Command 0x{command:X2} is not the live data response.");
        }

        var reading = new EcoWittLiveReading();
        var problems = new List<string>();
        var at = 0;
        while (at < data.Length)
        {
            var id = data[at];
            if (!_fields.TryGetValue(id, out var field))
            {
                problems.Add($"Field 0x{id:X2} at offset {at} is not known, so the {data.Length - at} bytes from it were not decoded");
                break;
            }

            if (at + 1 + field.Size > data.Length)
            {
                problems.Add($"Field 0x{id:X2} at offset {at} needs {field.Size} bytes but only {data.Length - at - 1} remain");
                break;
            }

            reading = field.Apply(reading, Value(data.Slice(at + 1, field.Size), field) / field.Divisor);
            at += 1 + field.Size;
        }

        return reading with { Problems = [.. problems.Take(_maxProblems)] };
    }

    private static double Value(ReadOnlySpan<byte> bytes, Field field)
    {
        long value = 0;
        foreach (var b in bytes)
        {
            value = (value << 8) | b;
        }

        if (field.Signed && bytes.Length == 2)
        {
            value = (short)value;
        }

        return value;
    }

    private sealed record Field(int Size, double Divisor, bool Signed, Func<EcoWittLiveReading, double, EcoWittLiveReading> Apply);
}
