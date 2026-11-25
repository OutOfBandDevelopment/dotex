using Microsoft.EntityFrameworkCore;
using OoBDev.Data.EntityFrameworkCore;

namespace OoBDev.ComplexEvents.EntityFrameworkCore;

/// <summary>The schedule table. The consumer picks the database provider.</summary>
/// <param name="options">The context options.</param>
public sealed class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    /// <summary>The scheduled items.</summary>
    public DbSet<ScheduleRow> Schedule => Set<ScheduleRow>();

    /// <summary>The last run of each recurring schedule.</summary>
    public DbSet<LastRunRow> LastRuns => Set<LastRunRow>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var instance = StoreInstance.Of(this);
        modelBuilder.Entity<LastRunRow>(e =>
        {
            instance.ToTable(e, "ComplexEventLastRun");
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(200);
        });

        modelBuilder.Entity<ScheduleRow>(e =>
        {
            instance.ToTable(e, "ComplexEventSchedule");
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Key).IsUnique();
            e.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            e.HasIndex(x => new { x.Failed, x.DueAtTicks });
        });
    }
}
