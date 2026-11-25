using System;
using System.Collections.Generic;
using System.Globalization;

namespace OoBDev.Communications.Abstractions;

/// <summary>How and when one target wants to be reached.</summary>
/// <param name="TargetId">The target.</param>
/// <param name="Channels">Channel names in order of use, for example <c>mail</c> and <c>sms</c>; empty means no message.</param>
/// <param name="Culture">The language for composed messages.</param>
/// <param name="TimeZone">The zone for quiet hours.</param>
/// <param name="MinimumPriority">Requests below this priority are dropped.</param>
/// <param name="QuietHoursStart">Start of the quiet window in the target's local time, or null for none.</param>
/// <param name="QuietHoursEnd">End of the quiet window in the target's local time; the window may cross midnight.</param>
public sealed record TargetPreference(
    string TargetId,
    IReadOnlyList<string> Channels,
    CultureInfo Culture,
    TimeZoneInfo TimeZone,
    RequestPriority MinimumPriority = RequestPriority.Normal,
    TimeOnly? QuietHoursStart = null,
    TimeOnly? QuietHoursEnd = null);
