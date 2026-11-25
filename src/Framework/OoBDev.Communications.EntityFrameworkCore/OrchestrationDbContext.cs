using Microsoft.EntityFrameworkCore;
using OoBDev.Data.EntityFrameworkCore;

namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>The tables of the communication orchestrator: held requests and target preferences.</summary>
/// <param name="options">The context options.</param>
public sealed class OrchestrationDbContext(DbContextOptions<OrchestrationDbContext> options) : DbContext(options)
{
    /// <summary>Gets the held requests.</summary>
    public DbSet<DeferredRequestRow> DeferredRequests => Set<DeferredRequestRow>();

    /// <summary>Gets the target preferences.</summary>
    public DbSet<TargetPreferenceRow> TargetPreferences => Set<TargetPreferenceRow>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var instance = StoreInstance.Of(this);
        modelBuilder.Entity<DeferredRequestRow>(e =>
        {
            instance.ToTable(e, "DeferredRequests");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.DueAtTicks);
            e.Property(x => x.TargetId).HasMaxLength(200).IsRequired();
            e.Property(x => x.MessageType).HasMaxLength(200).IsRequired();
            e.Property(x => x.CorrelationId).HasMaxLength(200);
            e.Property(x => x.DataJson).IsRequired();
        });
        modelBuilder.Entity<TargetPreferenceRow>(e =>
        {
            instance.ToTable(e, "TargetPreferences");
            e.HasKey(x => x.TargetId);
            e.Property(x => x.TargetId).HasMaxLength(200);
            e.Property(x => x.Channels).HasMaxLength(500).IsRequired();
            e.Property(x => x.CultureName).HasMaxLength(20).IsRequired();
            e.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
        });
    }
}
