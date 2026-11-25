using System;
using System.Globalization;

namespace OoBDev.DevTerm.Configuration;

/// <summary>One line of the merged view: when it happened, on which device, and what moved which way.</summary>
public sealed record MergedLogEntry(DateTimeOffset Time, string Device, bool Sent, string Text)
{
    /// <summary><c>12:00:01.250 [scope] &lt; *IDN?</c> - <c>&gt;</c> for received, <c>&lt;</c> for sent.</summary>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Time.LocalDateTime:HH:mm:ss.fff} [{Device}] {(Sent ? "<" : ">")} {Text}");
}
