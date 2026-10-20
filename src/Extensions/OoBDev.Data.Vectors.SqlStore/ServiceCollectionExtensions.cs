using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Search.Semantic;

namespace OoBDev.Data.Vectors.SqlStore;

/// <summary>Registers the SQL Server vector store.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IVectorStoreProviderFactory"/> backed by SQL Server and binds <see cref="SqlVectorStoreOptions"/>.</summary>
    public static IServiceCollection TryAddSqlVectorStore(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string sectionName
#else
        string sectionName = nameof(SqlVectorStoreOptions)
#endif
        )
    {
        services.AddOptions<SqlVectorStoreOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations();
        services.TryAddTransient<IVectorStoreProviderFactory, SqlVectorStoreProviderFactory>();
        return services;
    }
}
