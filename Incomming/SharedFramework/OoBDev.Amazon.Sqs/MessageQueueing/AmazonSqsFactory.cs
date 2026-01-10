using Amazon;
using Amazon.SQS;
using OoBDev.MessageQueueing.Contracts.Services;

namespace OoBDev.Amazon.Sqs.MessageQueueing
{
    public class AmazonSqsFactory : IAmazonSqsFactory
    {
        public IAmazonSQS Create(IQueueConnectionString connection) =>
            Create(connection["AccessKeyId"], connection["SecretAccessKey"], RegionEndpoint.GetBySystemName(connection["Region"]));

        public IAmazonSQS Create(string accessKeyId, string secretAccessKey, RegionEndpoint region) =>
            new AmazonSQSClient(accessKeyId, secretAccessKey, region);
    }
}
