using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>Target preferences kept in the database.</summary>
/// <param name="factory">Creates a context per call.</param>
public sealed class EfTargetPreferenceProvider(IDbContextFactory<OrchestrationDbContext> factory) : ITargetPreferenceProvider
{
    /// <inheritdoc />
    public async Task<TargetPreference?> GetAsync(string targetId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var row = await db.TargetPreferences.AsNoTracking().SingleOrDefaultAsync(p => p.TargetId == targetId, cancellationToken).ConfigureAwait(false);
        return row is null ? null : ToPreference(row);
    }

    /// <summary>Stores the preference, replacing an earlier one for the same target.</summary>
    /// <param name="preference">The preference to keep.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>A task that completes when the preference is saved.</returns>
    public async Task SetAsync(TargetPreference preference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preference);
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var row = await db.TargetPreferences.SingleOrDefaultAsync(p => p.TargetId == preference.TargetId, cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new TargetPreferenceRow { TargetId = preference.TargetId };
            db.TargetPreferences.Add(row);
        }

        row.Channels = string.Join(',', preference.Channels);
        row.CultureName = preference.Culture.Name;
        row.TimeZoneId = preference.TimeZone.Id;
        row.MinimumPriority = (int)preference.MinimumPriority;
        row.QuietHoursStartTicks = preference.QuietHoursStart?.Ticks;
        row.QuietHoursEndTicks = preference.QuietHoursEnd?.Ticks;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static TargetPreference ToPreference(TargetPreferenceRow row) => new(
        row.TargetId,
        row.Channels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        CultureInfo.GetCultureInfo(row.CultureName),
        TimeZoneInfo.FindSystemTimeZoneById(row.TimeZoneId),
        (RequestPriority)row.MinimumPriority,
        row.QuietHoursStartTicks is { } start ? new TimeOnly(start) : null,
        row.QuietHoursEndTicks is { } end ? new TimeOnly(end) : null);
}
