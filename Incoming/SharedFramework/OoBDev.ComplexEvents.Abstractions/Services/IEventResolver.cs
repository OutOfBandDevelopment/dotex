
namespace OoBDev.ComplexEvents.Abstractions.Services
{
    public interface IEventResolver
    {
        string GetPartitionKey<TChannel>();
        string GetEventHubName<TChannel>();
        string GetMessageType(IEventData @event);
    }
}
