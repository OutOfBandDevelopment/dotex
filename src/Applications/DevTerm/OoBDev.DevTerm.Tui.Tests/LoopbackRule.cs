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
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// A single scripted command for <see cref="LoopbackTransport"/>: a regex matched against the
/// ASCII-decoded, terminator-stripped line a caller wrote, and a generator producing zero or more
/// response lines from that match. Each response line is pushed back as its own write, not
/// concatenated, so a line-buffering presenter (e.g. <c>AsciiPresenter</c>) sees them as separate
/// lines/events rather than one blob.
/// </summary>
internal sealed record LoopbackRule(Regex Pattern, Func<Match, IEnumerable<string>> Respond)
{
    /// <summary>An exact-text command producing one fixed response line, e.g. "hello" -&gt; "From Loopback test".</summary>
    public static LoopbackRule Literal(string command, string response) =>
        new(new Regex($"^{Regex.Escape(command)}$"), _ => [response]);

    /// <summary>A regex command whose capture groups feed the response generator, e.g. "Send Events: (\d+)".</summary>
    public static LoopbackRule Match(string pattern, Func<Match, IEnumerable<string>> respond) =>
        new(new Regex(pattern), respond);
}
