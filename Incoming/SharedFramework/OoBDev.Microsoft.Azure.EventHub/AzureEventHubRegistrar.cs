using OoBDev.Microsoft.Azure.EventHub.ComplexEvents;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Microsoft.Azure.EventHub
{
    public class AzureEventHubRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<IEventHubClientFactory, EventHubClientFactory>();
            services.AddTransient(typeof(IEventHubProvider<>), typeof(AzureEventHubProvider<>));
            return services;
        }
    }
}
