using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System;
using OoBDev.WorkQueue;

namespace OoBDev.PostgreSql.WorkQueue;

/// <summary>
/// Registers the PostgreSQL work queue.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="PostgresWorkQueue"/> as the <see cref="IWorkQueue"/>, binds <see cref="PostgresWorkQueueOptions"/>
    /// from <see cref="PostgresWorkQueueOptions.SectionName"/> and the shared <see cref="WorkQueueOptions"/>, and uses
    /// <see cref="OldestFirstClaimOrder"/> unless another <see cref="IPostgresClaimOrder"/> is registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddPostgresWorkQueue(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddWorkQueue(configuration);
        services.AddValidatedOptions<PostgresWorkQueueOptions>(configuration, PostgresWorkQueueOptions.SectionName);
        services.TryAddSingleton<IPostgresClaimOrder, OldestFirstClaimOrder>();
        services.TryAddSingleton<IWorkQueue, PostgresWorkQueue>();
        return services;
    }
}
