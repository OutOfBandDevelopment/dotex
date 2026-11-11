using Microsoft.EntityFrameworkCore;

namespace OoBDev.EventSourcing.EntityFrameworkCore;

/// <summary>
/// The context for the event and entity tables. One model serves PostgreSQL, SQL Server and SQLite;
/// only the JSON column types and the JSON check differ by provider.
/// </summary>
/// <param name="options">The context options.</param>
public sealed class EventSourcingDbContext(DbContextOptions<EventSourcingDbContext> options) : DbContext(options)
{
    private const string NpgsqlProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";
    private const string SqlServerProvider = "Microsoft.EntityFrameworkCore.SqlServer";
    private const string SqliteProvider = "Microsoft.EntityFrameworkCore.Sqlite";

    /// <summary>Gets the events.</summary>
    public DbSet<EventRow> Events => Set<EventRow>();

    /// <summary>Gets the entities.</summary>
    public DbSet<EntityRow> Entities => Set<EntityRow>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var provider = Database.ProviderName;

        modelBuilder.Entity<EventRow>(e =>
        {
            e.ToTable("Events");
            e.HasKey(x => x.Position);
            e.Property(x => x.Position).ValueGeneratedOnAdd();
            e.Property(x => x.StreamId).HasMaxLength(200).IsRequired();
            e.Property(x => x.Type).HasMaxLength(200).IsRequired();
            e.Property(x => x.Payload).IsRequired();
            e.HasIndex(x => new { x.StreamId, x.Sequence }).IsUnique();

            switch (provider)
            {
                case NpgsqlProvider:
                    e.Property(x => x.Payload).HasColumnType("jsonb");
                    e.Property(x => x.Metadata).HasColumnType("jsonb");
                    break;
                case SqlServerProvider:
                    e.Property(x => x.Payload).HasColumnType("nvarchar(max)");
                    e.Property(x => x.Metadata).HasColumnType("nvarchar(max)");
                    e.ToTable(t => t.HasCheckConstraint("CK_Events_Payload_Json", "ISJSON([Payload]) = 1"));
                    break;
                case SqliteProvider:
                    e.ToTable(t => t.HasCheckConstraint("CK_Events_Payload_Json", "json_valid(\"Payload\")"));
                    break;
            }
        });

        modelBuilder.Entity<EntityRow>(e =>
        {
            e.ToTable("Entities");
            e.HasKey(x => new { x.Type, x.Key });
            e.Property(x => x.Type).HasMaxLength(200);
            e.Property(x => x.Key).HasMaxLength(400);
            e.Property(x => x.Value).IsRequired();

            switch (provider)
            {
                case NpgsqlProvider:
                    e.Property(x => x.Value).HasColumnType("jsonb");
                    break;
                case SqlServerProvider:
                    e.Property(x => x.Value).HasColumnType("nvarchar(max)");
                    e.ToTable(t => t.HasCheckConstraint("CK_Entities_Value_Json", "ISJSON([Value]) = 1"));
                    break;
                case SqliteProvider:
                    e.ToTable(t => t.HasCheckConstraint("CK_Entities_Value_Json", "json_valid(\"Value\")"));
                    break;
            }
        });
    }
}
