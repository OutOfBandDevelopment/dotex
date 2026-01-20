using OoBDev.MessageQueueing.Contracts;
using OoBDev.MessageQueueing.Contracts.Services;
using OoBDev.Toolkit.Common;
using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.ServiceBus.MessageQueueing
{
    [MessageQueue(QueueType = QueueTypes.AzureServiceBusQueue)]
    [MessageQueue(QueueType = QueueTypes.AzureServiceBusTopic)]
    public class AzureServiceBusQueueMessageSender<TChannel> : IMessageSenderProvider<TChannel>
    {
        private readonly IQueueResolver<TChannel> _resolver;
        private readonly IObjectSerializer _serializer;

        private readonly Lazy<ServiceBusSender> _queue;

        public AzureServiceBusQueueMessageSender(
            IQueueResolver<TChannel> resolver,
            IQueueFactory<TChannel> factory,
            IObjectSerializer serializer
            )
        {
            _resolver = resolver;
            this._queue = new Lazy<ServiceBusSender>(() => factory.GetQueue(_resolver.GetQueueName()));
            _serializer = serializer;
        }

        public async Task<string> SendAsync<T>(T message, string messageId, IDictionary<string, object> properties) where T : class
        {
            var (contentType, data) = _serializer.Serialize(message);

            var request = new ServiceBusMessage(data)
            {
                CorrelationId = messageId,
                ContentType = contentType,
            };

            foreach (var property in properties.Where(p => p.Value != null))
            {
                request.ApplicationProperties.Add(property.Key, property.Value);
            }                

            await _queue.Value.SendMessageAsync(request);

            return messageId;
        }
    }
}
