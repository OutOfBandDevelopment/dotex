using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.DevTerm.Loopback;

/// <summary>The default script: the example commands the transport was designed around.</summary>
public static class LoopbackScript
{
    /// <summary>The lines <c>help</c> and <c>?</c> print.</summary>
    public static readonly IReadOnlyList<string> HelpLines =
    [
        "Commands:",
        "  hello                  -> From Loopback test",
        "  Send Stream: N, ascii  -> an N-character deterministic ASCII run",
        "  Send Events: N         -> N separate 'Event 1'..'Event N' lines",
        "  MEAS?                  -> the next simulated sensor sample (A=.. B=.. C=.. X=.. ...)",
        "  STATUS?                -> a labelled reading with units (temp=21.50 C volts=3.30 V state=OK)",
        "  Samples: N             -> the next N simulated sensor samples, one per line",
        "  help or ?              -> this list",
    ];

    /// <summary>A fresh script; the sample counters belong to the returned instance, so every connection starts at sample 0.</summary>
    public static IReadOnlyList<LoopbackRule> Default()
    {
        var nextSample = 0;
        var nextStatus = 0;
        return
        [
            LoopbackRule.Literal("hello", "From Loopback test"),
            LoopbackRule.Match(@"^Send Stream: (\d+), (\w+)$", m => [LoopbackGenerators.AsciiStream(int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))]),
            LoopbackRule.Match(@"^Send Events: (\d+)$", m => LoopbackGenerators.Events(int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)), streaming: true),
            LoopbackRule.Match(@"^MEAS\?$", _ => [LoopbackGenerators.SensorSample(nextSample++)]),
            LoopbackRule.Match(@"^STATUS\?$", _ => [LoopbackGenerators.Status(nextStatus++)]),
            LoopbackRule.Match(@"^Samples: (\d+)$", m =>
            {
                var count = Math.Clamp(int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture), 0, 1000);
                var first = nextSample;
                nextSample += count;
                return Enumerable.Range(first, count).Select(LoopbackGenerators.SensorSample).ToList();
            }, streaming: true),
            LoopbackRule.Match(@"^(help|\?)$", _ => HelpLines),
        ];
    }
}
