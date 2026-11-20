using Microsoft.EntityFrameworkCore;

namespace OoBDev.DataLoader.EntityFrameworkCore;

/// <summary>The loaded rows table. The consumer picks the database provider.</summary>
/// <param name="options">The context options.</param>
public sealed class LoadedRowsDbContext(DbContextOptions<LoadedRowsDbContext> options) : DbContext(options)
{
    /// <summary>The stored rows.</summary>
    public DbSet<LoadedRow> Rows => Set<LoadedRow>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoadedRow>(e =>
        {
            e.ToTable("LoadedRows");
            e.HasKey(x => new { x.Dataset, x.Key });
            e.Property(x => x.Dataset).HasMaxLength(100);
            e.Property(x => x.Key).HasMaxLength(300);
        });
    }
}
