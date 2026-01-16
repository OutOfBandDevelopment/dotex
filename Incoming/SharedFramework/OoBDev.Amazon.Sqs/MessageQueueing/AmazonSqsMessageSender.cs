using Amazon.SQS.Model;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.MessageQueueing.Contracts.Services;
using OoBDev.Toolkit.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Amazon.Sqs.MessageQueueing
{
    [MessageQueue(
        QueueType = QueueTypes.AmazonSimpleQueue
        )]
    public class AmazonSqsMessageSender<TChannel> : IMessageSenderProvider<TChannel>
    {
        private readonly IAmazonSqsFactory _factory;
        private readonly IQueueResolver<TChannel> _resolver;
        private readonly IObjectSerializer _serializer;
        private readonly IQueueConfig<TChannel> _config;

        public AmazonSqsMessageSender(
            IAmazonSqsFactory factory,
            IQueueResolver<TChannel> resolver,
            IObjectSerializer serializer,
            IQueueConfig<TChannel> config
            )
        {
            _factory = factory;
            _resolver = resolver;
            _config = config;
            _serializer = serializer;
        }

        public async Task<string> SendAsync<T>(T message, string messageId, IDictionary<string, object> properties) where T : class
        {
            var queueName = _resolver.GetQueueName();
            var client = _factory.Create(
                _config.AccessKeyId ?? throw new ApplicationException($"Missing SQS {nameof(_config.AccessKeyId)} for {queueName}"),
                _config.SecretAccessKey ?? throw new ApplicationException($"Missing SQS {nameof(_config.SecretAccessKey)} for {queueName}"),
                _config.Region);

            var queueUrl = await client.GetQueueUrlAsync(queueName);

            var (contentType, data) = _serializer.Serialize(message);
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl.QueueUrl,
                MessageBody = Encoding.UTF8.GetString(data),

                MessageAttributes =
                {
                    {"Content-Type", new MessageAttributeValue(){ DataType = "String",  StringValue = contentType } },
                    {"External-MessageId", new MessageAttributeValue(){ DataType = "String", StringValue = messageId } },
                },

                DelaySeconds = _config.DelaySeconds,
            };

            foreach (var property in properties.Where(p => p.Value != null))
                request.MessageAttributes.Add(property.Key, new MessageAttributeValue() { DataType = "String", StringValue = property.Value.ToString() });

            var sent = await client.SendMessageAsync(request);

            return sent.MessageId;
        }
    }
}
