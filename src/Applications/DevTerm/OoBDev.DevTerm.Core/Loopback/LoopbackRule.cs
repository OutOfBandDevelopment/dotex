using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OoBDev.DevTerm.Loopback;

/// <summary>
/// One scripted command: a regex matched against the terminator-stripped line a caller wrote, and a generator producing
/// zero or more response lines. Each response line is pushed back as its own write so a line-buffering presenter sees separate lines.
/// </summary>
public sealed record LoopbackRule(Regex Pattern, Func<Match, IEnumerable<string>> Respond, bool Streaming = false)
{
    /// <summary>An exact-text command with one fixed response line. Case-insensitive.</summary>
    public static LoopbackRule Literal(string command, string response) =>
        new(new Regex($"^{Regex.Escape(command)}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)), _ => [response]);

    /// <summary>
    /// A regex command whose capture groups feed the response generator. Case-insensitive. A <paramref name="streaming"/> rule has a
    /// naturally multi-line response, so <see cref="LoopbackTransportOptions.SampleIntervalMs"/> paces between its lines.
    /// </summary>
    public static LoopbackRule Match(string pattern, Func<Match, IEnumerable<string>> respond, bool streaming = false) =>
        new(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)), respond, streaming);
}
