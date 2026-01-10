using OoBDev.MessageQueueing.Contracts;

namespace OoBDev.Azure.ServiceBus.Tests.MessageQueueing
{
    [MessageQueue(QueueName = "test-topic", QueueType = QueueTypes.AzureServiceBusTopic)]
    public class TestTopicTarget
    {
    }
}
