// Ignore Spelling: Dac

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.DacFx;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddDacPacServices(this ServiceCollection services)
    {
        services.TryAddTransient<IDacPacBuilder, DacPacBuilder>();
        return services;
    }
}
