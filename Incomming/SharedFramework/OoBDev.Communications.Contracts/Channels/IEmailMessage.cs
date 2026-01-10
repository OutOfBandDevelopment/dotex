using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Contracts.Channels
{
    public interface IEmailMessage
    {
        string? MessageType { get; }

        ICollection<string> BccAddresses { get; }
        ICollection<string> CcAddresses { get; }
        string? FromAddress { get; }
        string? HtmlContent { get; }
        Guid RequestId { get; }
        string? Subject { get; }
        string? TextContent { get; }
        ICollection<string> ToAddresses { get; }

        IDictionary<string, object> Headers { get; }
    }
}