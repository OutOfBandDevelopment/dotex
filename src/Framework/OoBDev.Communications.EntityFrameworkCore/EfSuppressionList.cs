using Microsoft.EntityFrameworkCore;
using OoBDev.Communications.Abstractions;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>A suppression list that survives restarts and is shared by every instance. Addresses compare case-insensitively.</summary>
/// <param name="factory">Creates a context per call.</param>
/// <param name="timeProvider">The clock for the added time.</param>
public sealed class EfSuppressionList(IDbContextFactory<SuppressionsDbContext> factory, TimeProvider timeProvider) : ISuppressionList
{
    /// <inheritdoc />
    public async Task<bool> IsSuppressedAsync(string address, CancellationToken cancellationToken = default)
    {
        var key = Key(address);
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        return await db.Suppressions.AnyAsync(s => s.Address == key, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddAsync(string address, string reason, CancellationToken cancellationToken = default)
    {
        var key = Key(address);
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        db.Suppressions.Add(new SuppressionRow { Address = key, Reason = reason ?? string.Empty, AddedAtTicks = timeProvider.GetUtcNow().UtcTicks });
        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            if (!await db.Suppressions.AsNoTracking().AnyAsync(s => s.Address == key, cancellationToken).ConfigureAwait(false))
            {
                throw;
            }

            // Already suppressed (a concurrent add won); the first reason stays.
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string address, CancellationToken cancellationToken = default)
    {
        var key = Key(address);
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        await db.Suppressions.Where(s => s.Address == key).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string Key(string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        return address.Trim().ToLower(CultureInfo.InvariantCulture);
    }
}
