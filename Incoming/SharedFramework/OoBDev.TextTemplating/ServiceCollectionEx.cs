using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.TextTemplating
{
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddTextTemplatingServices(this IServiceCollection services) =>
            new TextTemplatingRegistrar().AddServices(services);
    }
}
