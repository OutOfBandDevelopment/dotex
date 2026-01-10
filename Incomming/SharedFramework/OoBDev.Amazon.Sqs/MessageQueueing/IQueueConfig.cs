using Amazon;

namespace OoBDev.Amazon.Sqs.MessageQueueing
{
    public interface IQueueConfig<TChannel>
    {
        RegionEndpoint Region { get; }
        string? AccessKeyId { get; }
        string? SecretAccessKey { get; }

        int MaxNumberOfMessages { get; }
        int WaitTimeSeconds { get; }
        int DelaySeconds { get; }

        int LeadOutSeconds { get; }
    }
}