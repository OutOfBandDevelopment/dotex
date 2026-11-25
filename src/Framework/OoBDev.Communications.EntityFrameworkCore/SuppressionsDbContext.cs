using Microsoft.EntityFrameworkCore;
using OoBDev.Data.EntityFrameworkCore;

namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>The suppression table. The consumer picks the database provider.</summary>
/// <param name="options">The context options.</param>
public sealed class SuppressionsDbContext(DbContextOptions<SuppressionsDbContext> options) : DbContext(options)
{
    /// <summary>The suppressed addresses.</summary>
    public DbSet<SuppressionRow> Suppressions => Set<SuppressionRow>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SuppressionRow>(e =>
        {
            var instance = StoreInstance.Of(this);
            instance.ToTable(e, "Suppressions");
            e.HasKey(x => x.Address);
            e.Property(x => x.Address).HasMaxLength(320);
            e.Property(x => x.Reason).HasMaxLength(200).IsRequired();
        });
    }
}
