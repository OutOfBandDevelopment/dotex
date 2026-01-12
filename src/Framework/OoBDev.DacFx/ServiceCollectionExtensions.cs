// Ignore Spelling: Dac

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.DacFx;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddDacPacServices(this IServiceCollection services)
    {
        services.TryAddTransient<IDacPacBuilder, DacPacBuilder>();
        services.TryAddTransient<IDacPacValidator, DacPacValidator>();
        return services;
    }
}
