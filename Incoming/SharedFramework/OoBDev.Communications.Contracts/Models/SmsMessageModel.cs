
using OoBDev.Communications.Contracts.Channels;
using OoBDev.MessageQueueing.Contracts;
using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Contracts.Models
{
    [MessageQueue(
        QueueName = SystemQueues.SendSms,
        QueueType = QueueTypes.AzureStorageQueue
        )]
    public class SmsMessageModel : ISmsMessage
    {
        //TODO: we need model validation
        public string? MessageType { get; set; }
        public string? Body { get; set; }
        public string? To { get; set; }
        public string? From { get; set; }
        public Guid RequestId { get; set; }
        public IDictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
    }
}
