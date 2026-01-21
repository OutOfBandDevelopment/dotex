namespace OoBDev.Microsoft.Azure.EventHub.ComplexEvents
{
    public interface IEventHubClientFactory
    {
        IEventHubClient Create(string eventHub, string partitionKey);
    }
}