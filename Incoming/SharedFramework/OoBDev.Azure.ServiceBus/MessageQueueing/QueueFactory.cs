using OoBDev.MessageQueueing.Contracts.Services;
using Azure.Messaging.ServiceBus;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Azure.ServiceBus.MessageQueueing
{
    [ExcludeFromCodeCoverage]
    public class QueueFactory<TChannel> : IQueueFactory<TChannel>
    {
        private readonly IQueueResolver<TChannel> _resolver;

        public QueueFactory(
            IQueueResolver<TChannel> resolver
            )
        {
            _resolver = resolver;
        }

        public ServiceBusSender GetQueue(string queueName) =>
            new ServiceBusClient(
                _resolver.GetConnectionString().ToString()
                ).CreateSender(queueName);
    }
}