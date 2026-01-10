using Azure.Messaging.ServiceBus;

namespace OoBDev.Azure.ServiceBus.MessageQueueing
{
    public interface IQueueFactory<Q>
    {
        ServiceBusSender GetQueue(string queueName);
    }
}