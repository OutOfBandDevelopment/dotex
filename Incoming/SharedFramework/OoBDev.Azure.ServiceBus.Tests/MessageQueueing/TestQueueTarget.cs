using OoBDev.MessageQueueing.Contracts;

namespace OoBDev.Azure.ServiceBus.Tests.MessageQueueing
{
    [MessageQueue(QueueName = "test-queue", QueueType = QueueTypes.AzureServiceBusQueue)]
    public class TestQueueTarget
    {
    }
}
