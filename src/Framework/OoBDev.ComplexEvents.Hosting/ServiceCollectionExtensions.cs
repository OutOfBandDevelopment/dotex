using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace OoBDev.ComplexEvents.Hosting;

/// <summary>Extension methods that host the schedule runner.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds <see cref="ScheduleHost"/>, which runs due items from the registered <see cref="IScheduleStore"/> (in memory when none is registered).</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="run">Runs one item (deserialize by <see cref="ScheduleItem.EventType"/> and dispatch).</param>
    /// <param name="configure">Optional options changes.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddScheduleHosting(this IServiceCollection services, Func<ScheduleItem, CancellationToken, Task> run, Action<ScheduleHostOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(run);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IScheduleStore>(sp => new InMemoryScheduleStore(sp.GetRequiredService<TimeProvider>()));
        services.AddOptions<ScheduleHostOptions>();
        if (configure is not null) services.Configure(configure);
        services.TryAddSingleton(run);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, ScheduleHost>());
        return services;
    }
}
