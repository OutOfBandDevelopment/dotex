using Azure.Messaging.EventHubs;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.Toolkit.Common;
using OoBDev.Toolkit.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.EventHub.ComplexEvents
{
    [ServiceConfig(
        Priority = -10
        )]
    public class AzureEventHubProvider<TChannel> : IEventHubProvider<TChannel>
    {
        private readonly string _eventHubName;
        private readonly string _partitionKey;

        private readonly IObjectSerializer _serializer;
        private readonly IEventHubClient _client;

        public AzureEventHubProvider(
            IEventResolver resolver,
            IObjectSerializer serializer,
            IEventHubClientFactory factory
            )
        {
            _eventHubName = resolver.GetEventHubName<TChannel>();
            _partitionKey = resolver.GetPartitionKey<TChannel>();
            _serializer = serializer;
            _client = factory.Create(_eventHubName, _partitionKey) ??
                throw new NotSupportedException($"Unable to create IEventHubClient for {_eventHubName}/{_partitionKey}");
        }

        public async Task SendAsync<TEvent>(
            TEvent item,
            IDictionary<string, object> properties
            ) where TEvent : IEventData
        {
            var (contentType, payload) = _serializer.Serialize(item);

            var evnt = new EventData(payload)
            {
                ContentType = contentType
            };
            foreach (var property in properties)
            {
                evnt.Properties.Add(property.Key, property.Value);
            }
            await _client.SendAsync(evnt, _partitionKey).ConfigureAwait(false);
        }
    }
}
