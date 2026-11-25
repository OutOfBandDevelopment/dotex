using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Data.EntityFrameworkCore;

/// <summary>
/// Lets several copies of one store share a database: <see cref="UseStoreInstance"/> names the instance on the context options,
/// <see cref="Of"/> reads it in <c>OnModelCreating</c>, and a model cache key per instance keeps one model for each.
/// </summary>
public static class StoreInstance
{
    /// <summary>Names the instance of the store; <see langword="null"/> parts keep the defaults.</summary>
    /// <param name="builder">The options builder of the store context.</param>
    /// <param name="schema">The schema, for engines that have schemas.</param>
    /// <param name="tablePrefix">The table prefix, for engines without schemas.</param>
    /// <returns>The same builder.</returns>
    public static DbContextOptionsBuilder UseStoreInstance(this DbContextOptionsBuilder builder, string? schema = null, string? tablePrefix = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new Extension(new StoreInstanceOptions(schema, tablePrefix)));
        return builder.ReplaceService<IModelCacheKeyFactory, KeyFactory>();
    }

    /// <summary>The generic form of <see cref="UseStoreInstance(DbContextOptionsBuilder, string?, string?)"/>.</summary>
    /// <typeparam name="TContext">The store context.</typeparam>
    /// <param name="builder">The options builder.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="tablePrefix">The table prefix.</param>
    /// <returns>The same builder.</returns>
    public static DbContextOptionsBuilder<TContext> UseStoreInstance<TContext>(this DbContextOptionsBuilder<TContext> builder, string? schema = null, string? tablePrefix = null)
        where TContext : DbContext
    {
        UseStoreInstance((DbContextOptionsBuilder)builder, schema, tablePrefix);
        return builder;
    }

    /// <summary>The instance a context was configured with, or <see cref="StoreInstanceOptions.Default"/>.</summary>
    /// <param name="context">The context being modelled.</param>
    /// <returns>The instance options.</returns>
    public static StoreInstanceOptions Of(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.GetService<IDbContextOptions>().FindExtension<Extension>()?.Instance ?? StoreInstanceOptions.Default;
    }

    private sealed class Extension(StoreInstanceOptions instance) : IDbContextOptionsExtension
    {
        public StoreInstanceOptions Instance { get; } = instance;

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

            public override string LogFragment => $"Schema={extension.Instance.Schema} TablePrefix={extension.Instance.TablePrefix} ";

            public override int GetServiceProviderHashCode() => 0;

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                debugInfo["StoreInstance:Schema"] = extension.Instance.Schema ?? string.Empty;
                debugInfo["StoreInstance:TablePrefix"] = extension.Instance.TablePrefix ?? string.Empty;
            }
        }
    }

    private sealed class KeyFactory(ModelCacheKeyFactoryDependencies dependencies) : ModelCacheKeyFactory(dependencies)
    {
        public override object Create(DbContext context, bool designTime) => (base.Create(context, designTime), Of(context));
    }
}
