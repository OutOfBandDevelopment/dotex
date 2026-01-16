
namespace OoBDev.ComplexEvents.Contracts.Services
{
    public interface IEventResolver
    {
        string GetPartitionKey<TChannel>();
        string GetEventHubName<TChannel>();
        string GetMessageType(IEventData @event);
    }
}
