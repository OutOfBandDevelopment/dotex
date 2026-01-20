using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Microsoft.Azure.EventHub.ComplexEvents
{
    [ExcludeFromCodeCoverage]
    public class EventHubClientFactory : IEventHubClientFactory
    {
        private readonly string _connectionString;

        public EventHubClientFactory(
            IConfiguration config
            ) =>
            _connectionString = config["Azure:EventHub:Default:ConnectionString"];

        public IEventHubClient Create(string eventHub, string partitionKey) =>
            new EventHubClientWrapped(
            string.IsNullOrWhiteSpace(this._connectionString) ? null :
            new EventHubProducerClient(this._connectionString)
        );
    }
}