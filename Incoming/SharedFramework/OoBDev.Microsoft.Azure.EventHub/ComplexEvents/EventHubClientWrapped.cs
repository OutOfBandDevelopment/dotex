using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.EventHub.ComplexEvents
{
    public class EventHubClientWrapped : IEventHubClient
    {
        private readonly EventHubProducerClient? _wrapped;
        public EventHubClientWrapped(
             EventHubProducerClient? wrapped
            ) => _wrapped = wrapped;

        public Task SendAsync(EventData evnt, string partitionKey) =>
            _wrapped?.SendAsync(new[] { evnt }) ??
            throw new NullReferenceException($"No EventHubClient provided for \"{evnt.ContentType}\"@{partitionKey}")
            ;
    }
}
