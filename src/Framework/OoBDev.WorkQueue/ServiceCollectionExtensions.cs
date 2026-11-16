using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.System;

namespace OoBDev.WorkQueue;

/// <summary>
/// Registers the work queue options, the in-memory queue and stage workers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="WorkQueueOptions"/> from the <see cref="WorkQueueOptions.SectionName"/> section and registers <see cref="TimeProvider.System"/> when none exists.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddWorkQueue(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<WorkQueueOptions>(configuration, WorkQueueOptions.SectionName);
        services.TryAddSingleton(TimeProvider.System);
        return services;
    }

    /// <summary>Registers <see cref="InMemoryWorkQueue"/> as the <see cref="IWorkQueue"/> when none is registered.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddInMemoryWorkQueue(this IServiceCollection services)
    {
        services.AddOptions<WorkQueueOptions>();
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IWorkQueue, InMemoryWorkQueue>();
        return services;
    }

    /// <summary>
    /// Adds a hosted <see cref="StageWorker"/> for a stage. The worker runs only while the stage is enabled in <see cref="WorkQueueOptions.Stages"/>
    /// (<c>WorkQueue:Stages:{stage}:Enabled</c>).
    /// </summary>
    /// <typeparam name="THandler">The stage handler, resolved from the container.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="stage">The stage name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWorkStage<THandler>(this IServiceCollection services, string stage)
        where THandler : class, IWorkStageHandler
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stage);
        services.TryAddTransient<THandler>();
        services.AddSingleton<IHostedService>(provider => new StageWorker(
            stage,
            provider.GetRequiredService<IWorkQueue>(),
            provider.GetRequiredService<THandler>(),
            provider.GetRequiredService<IOptions<WorkQueueOptions>>(),
            provider.GetRequiredService<TimeProvider>(),
            provider.GetRequiredService<ILogger<StageWorker>>()));
        return services;
    }
}
