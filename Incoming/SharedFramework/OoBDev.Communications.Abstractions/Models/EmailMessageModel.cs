using OoBDev.Communications.Contracts.Channels;
using OoBDev.MessageQueueing.Contracts;
using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions.Models
{
    [MessageQueue(
        QueueName = SystemQueues.SendEmail,
        QueueType = QueueTypes.AzureStorageQueue
        )]
    public class EmailMessageModel : IEmailMessage
    {
        public string? MessageType { get; set; }
        public string? FromAddress { get; set; }

        public ICollection<string> ToAddresses { get; set; } = new List<string>();
        public ICollection<string> CcAddresses { get; set; } = new List<string>();
        public ICollection<string> BccAddresses { get; set; } = new List<string>();

        public string? Subject { get; set; }

        public string? TextContent { get; set; }
        public string? HtmlContent { get; set; }
        public Guid RequestId { get; set; }

        public IDictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
    }
}