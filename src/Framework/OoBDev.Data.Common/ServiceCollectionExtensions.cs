using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Data.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddDataCommonServices(this IServiceCollection services)
    {
        services.AddTransient(typeof(IDatabaseQuery<>), typeof(DatabaseQuery<>));
        return services;
    }
}
