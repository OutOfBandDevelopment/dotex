using Microsoft.EntityFrameworkCore;
using System;

namespace OoBDev.MessageQueueing.EntityFrameworkCore;

/// <summary>Puts the outbox table into a model, so a business context can write messages in its own transaction.</summary>
public static class OutboxModelExtensions
{
    /// <summary>
    /// Maps <see cref="OutboxRow"/> to the <c>Outbox</c> table. Call it in <c>OnModelCreating</c> of the business context; the
    /// dispatcher's <see cref="EfOutboxStore"/> reads the same table when both use one database.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The model builder for chaining.</returns>
    public static ModelBuilder AddOutbox(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.Entity<OutboxRow>(e =>
        {
            e.ToTable("Outbox");
            e.HasKey(x => x.Position);
            e.Property(x => x.Position).ValueGeneratedOnAdd();
            e.Property(x => x.Id).HasMaxLength(200).IsRequired();
            e.Property(x => x.TypeName).HasMaxLength(500).IsRequired();
            e.Property(x => x.Json).IsRequired();
            e.HasIndex(x => x.Id).IsUnique();
        });
        return modelBuilder;
    }
}
