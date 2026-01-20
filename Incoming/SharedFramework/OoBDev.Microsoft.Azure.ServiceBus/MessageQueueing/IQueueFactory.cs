using Azure.Messaging.ServiceBus;

namespace OoBDev.Microsoft.Azure.ServiceBus.MessageQueueing
{
    public interface IQueueFactory<Q>
    {
        ServiceBusSender GetQueue(string queueName);
    }
}