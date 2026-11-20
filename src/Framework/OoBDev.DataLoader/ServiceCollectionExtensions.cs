using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.DataLoader;

/// <summary>Registers the data loader.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the CSV and JSON readers and <see cref="ILoader"/>. Register an <see cref="IRowSink"/> and any
    /// <see cref="IPipelineHandler"/> instances separately; the loader is not resolvable without a sink.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection TryAddDataLoader(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IRowReader, CsvRowReader>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IRowReader, JsonRowReader>());
        services.TryAddTransient<ILoader, Loader>();
        return services;
    }
}
