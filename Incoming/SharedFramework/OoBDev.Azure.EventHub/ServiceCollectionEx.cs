using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Azure.EventHub
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddAzureEventHubServices(this IServiceCollection services) =>
            new AzureEventHubRegistrar().AddServices(services);
    }
}
