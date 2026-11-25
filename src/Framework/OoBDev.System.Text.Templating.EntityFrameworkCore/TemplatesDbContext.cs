using Microsoft.EntityFrameworkCore;
using OoBDev.Data.EntityFrameworkCore;

namespace OoBDev.System.Text.Templating.EntityFrameworkCore;

/// <summary>The template table. The consumer picks the database provider in <see cref="ServiceCollectionExtensions.TryAddEntityFrameworkTemplates"/>.</summary>
/// <param name="options">The context options.</param>
public sealed class TemplatesDbContext(DbContextOptions<TemplatesDbContext> options) : DbContext(options)
{
    /// <summary>Every version of every template.</summary>
    public DbSet<TemplateRow> Templates => Set<TemplateRow>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var instance = StoreInstance.Of(this);
        modelBuilder.Entity<TemplateRow>(e =>
        {
            instance.ToTable(e, "Templates");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Culture).HasMaxLength(20).IsRequired();
            e.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            e.Property(x => x.TargetContentType).HasMaxLength(100).IsRequired();
            e.Property(x => x.TargetFileExtension).HasMaxLength(20).IsRequired();
            e.Property(x => x.Content).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.Name, x.Culture, x.TenantId, x.Version }).IsUnique();
        });
    }
}
