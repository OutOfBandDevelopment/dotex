using Microsoft.EntityFrameworkCore;
using OoBDev.Data.EntityFrameworkCore;

namespace OoBDev.AspNetCore.EntityFrameworkCore;

/// <summary>The <c>RightAudit</c> table.</summary>
/// <param name="options">The context options.</param>
public sealed class RightAuditDbContext(DbContextOptions<RightAuditDbContext> options) : DbContext(options)
{
    /// <summary>Gets the stored checks.</summary>
    public DbSet<RightAuditRow> RightAudit => Set<RightAuditRow>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<RightAuditRow>(e =>
        {
            StoreInstance.Of(this).ToTable(e, "RightAudit");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            e.Property(x => x.Rights).HasMaxLength(1000).IsRequired();
            e.Property(x => x.Result).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.AtTicks);
            e.HasIndex(x => new { x.Subject, x.AtTicks });
        });
}
