using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions.Channels
{
    public interface ISmsMessage
    {
        string? MessageType { get; }

        string? Body { get; }
        string? From { get; }
        Guid RequestId { get; }
        string? To { get; }

        IDictionary<string, object> Headers { get; }
    }
}