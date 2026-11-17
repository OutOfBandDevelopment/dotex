using Microsoft.EntityFrameworkCore;
using OoBDev.MessageQueueing.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.EntityFrameworkCore;

/// <summary>
/// Relational <see cref="IOutboxStore"/>: a message is stored as its registered type name and JSON, read back in insert
/// order, and the row is deleted once sent (the same as the in-memory store).
/// </summary>
/// <param name="factory">Creates a context per call.</param>
/// <param name="types">The message types allowed in the outbox.</param>
public sealed class EfOutboxStore(IDbContextFactory<MessagingDbContext> factory, OutboxMessageTypes types) : IOutboxStore
{
    /// <inheritdoc />
    public async Task AddAsync(OutboxEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var row = new OutboxRow
        {
            Id = entry.Id,
            TypeName = types.RequireName(entry.Message),
            Json = JsonSerializer.Serialize(entry.Message, entry.Message.GetType()),
            CreatedAtTicks = entry.CreatedAt.UtcTicks,
        };
        var db = await factory.CreateDbContextAsync().ConfigureAwait(false);
        await using var dbDisposer = db.ConfigureAwait(false);
        db.Outbox.Add(row);
        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int max)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(max, 1);
        var db = await factory.CreateDbContextAsync().ConfigureAwait(false);
        await using var dbDisposer = db.ConfigureAwait(false);
        var rows = await db.Outbox.AsNoTracking().OrderBy(x => x.Position).Take(max).ToListAsync().ConfigureAwait(false);
        var entries = new List<OutboxEntry>(rows.Count);
        foreach (var row in rows)
        {
            var message = JsonSerializer.Deserialize(row.Json, types.Resolve(row.TypeName))
                ?? throw new InvalidOperationException($"Outbox entry '{row.Id}' has no message.");
            entries.Add(new OutboxEntry(row.Id, message, new DateTimeOffset(row.CreatedAtTicks, TimeSpan.Zero)));
        }

        return entries;
    }

    /// <inheritdoc />
    public async Task MarkSentAsync(string id)
    {
        var db = await factory.CreateDbContextAsync().ConfigureAwait(false);
        await using var dbDisposer = db.ConfigureAwait(false);
        await db.Outbox.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }
}
