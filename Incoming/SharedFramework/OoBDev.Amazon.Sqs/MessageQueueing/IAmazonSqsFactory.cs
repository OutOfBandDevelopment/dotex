using Amazon;
using Amazon.SQS;
using OoBDev.MessageQueueing.Contracts.Services;

namespace OoBDev.Amazon.Sqs.MessageQueueing
{
    public interface IAmazonSqsFactory
    {
        IAmazonSQS Create(IQueueConnectionString connection);
        IAmazonSQS Create(string accessKeyId, string secretAccessKey, RegionEndpoint region);
    }
}
