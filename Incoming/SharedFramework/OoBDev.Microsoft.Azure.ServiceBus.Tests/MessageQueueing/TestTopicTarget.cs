using OoBDev.MessageQueueing.Contracts;

namespace OoBDev.Microsoft.Azure.ServiceBus.Tests.MessageQueueing
{
    [MessageQueue(QueueName = "test-topic", QueueType = QueueTypes.AzureServiceBusTopic)]
    public class TestTopicTarget
    {
    }
}
