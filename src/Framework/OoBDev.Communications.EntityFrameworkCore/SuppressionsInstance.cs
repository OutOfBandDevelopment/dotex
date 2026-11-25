using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>Places the suppression table in a schema (PostgreSQL, SQL Server) or behind a table prefix (SQLite), so several copies share one database.</summary>
public static class SuppressionsInstance
{
    /// <summary>Names one instance of the store; <see langword="null"/> parts keep the defaults.</summary>
    /// <param name="builder">The options builder of <see cref="SuppressionsDbContext"/>.</param>
    /// <param name="schema">The schema, for engines that have schemas.</param>
    /// <param name="tablePrefix">The table prefix, for engines without schemas.</param>
    /// <returns>The same builder.</returns>
    public static DbContextOptionsBuilder UseSuppressionsInstance(this DbContextOptionsBuilder builder, string? schema = null, string? tablePrefix = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new Extension(schema, tablePrefix));
        return builder.ReplaceService<IModelCacheKeyFactory, KeyFactory>();
    }

    internal static (string? Schema, string? TablePrefix) Of(DbContext context)
    {
        var extension = context.GetService<IDbContextOptions>().FindExtension<Extension>();
        return (extension?.Schema, extension?.TablePrefix);
    }

    private sealed class Extension(string? schema, string? tablePrefix) : IDbContextOptionsExtension
    {
        public string? Schema { get; } = schema;

        public string? TablePrefix { get; } = tablePrefix;

        public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

        public void ApplyServices(IServiceCollection services)
        {
        }

        public void Validate(IDbContextOptions options)
        {
        }

        private sealed class ExtensionInfo(Extension extension) : DbContextOptionsExtensionInfo(extension)
        {
            public override bool IsDatabaseProvider => false;

            public override string LogFragment => $"Schema={extension.Schema} TablePrefix={extension.TablePrefix} ";

            public override int GetServiceProviderHashCode() => 0;

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                debugInfo["Suppressions:Schema"] = extension.Schema ?? string.Empty;
                debugInfo["Suppressions:TablePrefix"] = extension.TablePrefix ?? string.Empty;
            }
        }
    }

    private sealed class KeyFactory(ModelCacheKeyFactoryDependencies dependencies)
        : ModelCacheKeyFactory(dependencies)
    {
        public override object Create(DbContext context, bool designTime) => (base.Create(context, designTime), Of(context));
    }
}
