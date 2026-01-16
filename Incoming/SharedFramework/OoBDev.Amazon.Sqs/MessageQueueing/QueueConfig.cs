using Amazon;
using OoBDev.Extensions;
using OoBDev.MessageQueueing.Contracts.Services;

namespace OoBDev.Amazon.Sqs.MessageQueueing
{
    public class QueueConfig<TChannel> : IQueueConfig<TChannel>
    {
        public QueueConfig(
            IQueueResolver<TChannel>  _resolver
            )
        {
            var connectionString = _resolver?.GetConnectionString();

            var region = connectionString?[nameof(Region)];
            Region = !string.IsNullOrEmpty(region) ? RegionEndpoint.GetBySystemName(region) : RegionEndpoint.USEast1;
            AccessKeyId = connectionString?[nameof(AccessKeyId)];
            SecretAccessKey = connectionString?[nameof(SecretAccessKey)];

            string? config(string key) => _resolver?.GetConfigurationValue(key);

            MaxNumberOfMessages = config(nameof(MaxNumberOfMessages)).ToInteger(@default: 10, min: 0, max: 10);
            WaitTimeSeconds = config(nameof(WaitTimeSeconds)).ToInteger(@default: 20, min: 0, max: 20);
            DelaySeconds = config(nameof(DelaySeconds)).ToInteger(@default: 0, min: 0, max: 900);
            LeadOutSeconds = config(nameof(LeadOutSeconds)).ToInteger(@default: 10, min: 5, max: 5 * 60);
        }

        public RegionEndpoint Region { get; }

        public string? AccessKeyId { get; }

        public string? SecretAccessKey { get; }

        public int MaxNumberOfMessages { get; }
        public int WaitTimeSeconds { get; }
        public int DelaySeconds { get; }

        public int LeadOutSeconds { get; }
    }
}