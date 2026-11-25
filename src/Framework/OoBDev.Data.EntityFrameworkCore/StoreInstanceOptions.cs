using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OoBDev.Data.EntityFrameworkCore;

/// <summary>Where one instance of a store keeps its tables: a schema (PostgreSQL, SQL Server) or a table prefix (SQLite).</summary>
/// <param name="Schema">The schema, or <see langword="null"/> for the provider default.</param>
/// <param name="TablePrefix">Text put before every table name, or <see langword="null"/> for none.</param>
public sealed record StoreInstanceOptions(string? Schema = null, string? TablePrefix = null)
{
    /// <summary>The default instance: no schema and no prefix.</summary>
    public static StoreInstanceOptions Default { get; } = new();

    /// <summary>Places an entity in this instance.</summary>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="table">The table name of the default instance.</param>
    /// <returns>The same builder.</returns>
    public EntityTypeBuilder<T> ToTable<T>(EntityTypeBuilder<T> builder, string table)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ToTable(TablePrefix + table, Schema);
    }
}
