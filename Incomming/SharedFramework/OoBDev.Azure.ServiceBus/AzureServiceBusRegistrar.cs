using OoBDev.Azure.ServiceBus.MessageQueueing;
using OoBDev.MessageQueueing.Contracts.Services;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Azure.ServiceBus
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class AzureServiceBusRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.AddTransient(typeof(IMessageSenderProvider<>), typeof(AzureServiceBusQueueMessageSender<>));
            services.TryAddTransient(typeof(IQueueFactory<>), typeof(QueueFactory<>));
            return services;
        }
    }
}