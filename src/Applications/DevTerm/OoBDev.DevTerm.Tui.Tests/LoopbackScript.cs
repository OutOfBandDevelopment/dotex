using System.Collections.Generic;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>The default script — the example commands this transport was designed around.</summary>
internal static class LoopbackScript
{
    public static IReadOnlyList<LoopbackRule> Default() =>
    [
        LoopbackRule.Literal("hello", "From Loopback test"),
        LoopbackRule.Match(@"^Send Stream: (\d+), (\w+)$", m => [LoopbackGenerators.AsciiStream(int.Parse(m.Groups[1].Value))]),
        LoopbackRule.Match(@"^Send Events: (\d+)$", m => LoopbackGenerators.Events(int.Parse(m.Groups[1].Value))),
    ];
}
