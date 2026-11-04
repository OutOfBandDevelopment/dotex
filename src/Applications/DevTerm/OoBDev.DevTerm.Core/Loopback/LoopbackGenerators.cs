using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.DevTerm.Loopback;

/// <summary>Deterministic response generators for <see cref="LoopbackRule"/>: no <see cref="Random"/>, so a given input always produces the same bytes.</summary>
public static class LoopbackGenerators
{
    /// <summary>A fixed-length, repeating A-Z run.</summary>
    public static string AsciiStream(int length) =>
        string.Concat(Enumerable.Range(0, length).Select(i => (char)('A' + (i % 26))));

    /// <summary>"Event 1".."Event N", one per response line.</summary>
    public static IEnumerable<string> Events(int count) =>
        Enumerable.Range(1, count).Select(i => $"Event {i}");

    /// <summary>
    /// Status reading number <paramref name="index"/>, with units: <c>temp=21.50 C volts=3.30 V state=OK</c>.
    /// The temperature drifts and <c>state</c> reads <c>WARN</c> every fifth reading.
    /// </summary>
    public static string Status(int index) =>
        string.Create(CultureInfo.InvariantCulture,
            $"temp={21.5 + (0.25 * (index % 8)):0.00} C volts={3.3 - (0.01 * (index % 5)):0.00} V state={(index % 5 == 4 ? "WARN" : "OK")}");

    /// <summary>
    /// Simulated sensor sample number <paramref name="index"/>, a pure function of the index: three 0-100 channels
    /// (<c>A</c> sine, <c>B</c> slower cosine, <c>C</c> sawtooth), an x/y/z point inside ±1, a polar <c>R</c> (0-1) and
    /// <c>T</c> (degrees), and a hue <c>H</c> in degrees, for example <c>A=50.00 B=90.00 C=0.00 X=0.80 Y=0.00 Z=0.00 R=0.50 T=0.00 H=0.00</c>.
    /// </summary>
    public static string SensorSample(int index)
    {
        var n = (double)index;
        var values = new (string Name, double Value)[]
        {
            ("A", 50 + (40 * Math.Sin(n * 0.3))),
            ("B", 50 + (40 * Math.Cos(n * 0.13))),
            ("C", index * 7 % 100),
            ("X", 0.8 * Math.Cos(n * 0.4)),
            ("Y", 0.8 * Math.Sin(n * 0.4)),
            ("Z", Math.Sin(n * 0.1)),
            ("R", 0.5 + (0.4 * Math.Sin(n * 0.25))),
            ("T", index * 15 % 360),
            ("H", index * 10 % 360),
        };
        return string.Join(' ', values.Select(v => string.Create(CultureInfo.InvariantCulture, $"{v.Name}={v.Value:0.00}")));
    }
}
