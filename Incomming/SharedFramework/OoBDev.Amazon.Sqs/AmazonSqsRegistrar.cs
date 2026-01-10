using OoBDev.Amazon.Sqs.MessageQueueing;
using OoBDev.MessageQueueing.Contracts.Services;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Amazon.Sqs
{
    [ExcludeFromCodeCoverage]
    public class AmazonSqsRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.AddTransient(typeof(IMessageSenderProvider<>), typeof(AmazonSqsMessageSender<>));
            services.TryAddTransient(typeof(IQueueConfig<>), typeof(QueueConfig<>));
            services.TryAddTransient<IAmazonSqsFactory, AmazonSqsFactory>();
            return services;
        }
    }
}