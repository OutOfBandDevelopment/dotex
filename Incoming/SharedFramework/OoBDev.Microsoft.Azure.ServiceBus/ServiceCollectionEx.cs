using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Microsoft.Azure.ServiceBus
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class ServiceCollectionEx
    {
        public static IServiceCollection AddAzureServiceBusServices(this IServiceCollection services)
        {
            return new AzureServiceBusRegistrar().AddServices(services);
        }
    }
}
