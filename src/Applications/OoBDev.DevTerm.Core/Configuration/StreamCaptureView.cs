using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// Filter, search and sort for the Stream Monitor's single capture list (docs/specs/stream-monitor.md),
/// shared by both front ends so they show the same rows in the same order.
/// </summary>
public static class StreamCaptureView
{
    /// <summary>The distinct content types present, sorted, for a filter's choices.</summary>
    public static IReadOnlyList<string> Kinds(IEnumerable<StreamMonitorCapture> captures) =>
        [.. captures.Select(c => c.Capture.Kind.DisplayName).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase)];

    /// <summary>The distinct device names present, sorted.</summary>
    public static IReadOnlyList<string> Devices(IEnumerable<StreamMonitorCapture> captures) =>
        [.. captures.Select(c => c.DeviceName).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// One pane per device (simultaneous captures from different devices each get their own, decided 2026-10-13),
    /// preceded by the all-devices pane; devices sorted by name.
    /// </summary>
    public static IReadOnlyList<StreamDevicePane> Panes(IReadOnlyCollection<StreamMonitorCapture> captures)
    {
        ArgumentNullException.ThrowIfNull(captures);
        return
        [
            new StreamDevicePane(null, captures.Count),
            .. Devices(captures).Select(d => new StreamDevicePane(d, captures.Count(c => string.Equals(c.DeviceName, d, StringComparison.OrdinalIgnoreCase)))),
        ];
    }

    /// <summary>
    /// <paramref name="captures"/> narrowed to <paramref name="kind"/> and <paramref name="device"/> (null or
    /// empty = any) and to those whose device, content type, end state, file name or time contains
    /// <paramref name="search"/> (case-insensitive), in <paramref name="sort"/> order. Ties keep input order.
    /// </summary>
    public static IReadOnlyList<StreamMonitorCapture> Apply(
        IEnumerable<StreamMonitorCapture> captures,
        string? search = null,
        string? kind = null,
        string? device = null,
        StreamCaptureSort sort = StreamCaptureSort.Oldest)
    {
        ArgumentNullException.ThrowIfNull(captures);
        var query = captures.Where(c =>
            (string.IsNullOrEmpty(kind) || string.Equals(c.Capture.Kind.DisplayName, kind, StringComparison.OrdinalIgnoreCase))
            && (string.IsNullOrEmpty(device) || string.Equals(c.DeviceName, device, StringComparison.OrdinalIgnoreCase))
            && Matches(c, search));

        return sort switch
        {
            StreamCaptureSort.Newest => [.. query.OrderByDescending(c => c.LocalStartedAt)],
            StreamCaptureSort.Largest => [.. query.OrderByDescending(c => c.Capture.Data.Length)],
            StreamCaptureSort.Kind => [.. query.OrderBy(c => c.Capture.Kind.DisplayName, StringComparer.OrdinalIgnoreCase)],
            StreamCaptureSort.Device => [.. query.OrderBy(c => c.DeviceName, StringComparer.OrdinalIgnoreCase)],
            _ => [.. query.OrderBy(c => c.LocalStartedAt)],
        };
    }

    private static bool Matches(StreamMonitorCapture capture, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var haystack = string.Join(
            '\n',
            capture.DeviceName,
            capture.Capture.Kind.DisplayName,
            capture.EndLabel,
            capture.SavedPath is { } path ? Path.GetFileName(path) : string.Empty,
            capture.LocalStartedAt.ToString("yyyy-MM-dd HH:mm:ss", global::System.Globalization.CultureInfo.InvariantCulture));
        return haystack.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
