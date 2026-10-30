using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Text;
using System.Text.RegularExpressions;
using OoBDev.DevTerm;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
using System.IO.Pipelines;
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
