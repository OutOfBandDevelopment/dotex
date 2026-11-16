using Microsoft.EntityFrameworkCore;

namespace OoBDev.MessageQueueing.EntityFrameworkCore;

/// <summary>
/// The inbox and outbox tables. One model serves every relational provider; the consumer picks the provider in the
/// configure callback of <see cref="ServiceCollectionExtensions.TryAddEntityFrameworkMessaging"/>.
/// </summary>
/// <param name="options">The context options.</param>
public sealed class MessagingDbContext(DbContextOptions<MessagingDbContext> options) : DbContext(options)
{
    /// <summary>Messages a consumer has started.</summary>
    public DbSet<InboxRow> Inbox => Set<InboxRow>();

    /// <summary>Messages waiting to be sent.</summary>
    public DbSet<OutboxRow> Outbox => Set<OutboxRow>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboxRow>(e =>
        {
            e.ToTable("Inbox");
            e.HasKey(x => new { x.Consumer, x.MessageId });
            e.Property(x => x.Consumer).HasMaxLength(200);
            e.Property(x => x.MessageId).HasMaxLength(200);
        });

        modelBuilder.AddOutbox();
    }
}
