using OoBDev.MessageQueueing.Contracts;

namespace OoBDev.Microsoft.Azure.ServiceBus.Tests.MessageQueueing
{
    [MessageQueue(QueueName = "test-queue", QueueType = QueueTypes.AzureServiceBusQueue)]
    public class TestQueueTarget
    {
    }
}
