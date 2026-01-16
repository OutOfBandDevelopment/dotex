using Azure.Messaging.EventHubs;
using System.Threading.Tasks;

namespace OoBDev.Azure.EventHub.ComplexEvents
{
    public interface IEventHubClient
    {
        Task SendAsync(EventData evnt, string partitionKey);
    }
}
