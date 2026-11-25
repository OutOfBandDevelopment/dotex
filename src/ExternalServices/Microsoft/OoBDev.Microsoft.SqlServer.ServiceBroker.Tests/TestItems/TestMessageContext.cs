using Microsoft.Extensions.Configuration;
using OoBDev.MessageQueueing.Services;
using System;
using System.Collections.Generic;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.Tests.TestItems;

public sealed class TestMessageContext(IConfigurationSection config) : IMessageContext
{
    public string? OriginMessageId => null;
    public string? CorrelationId { get; set; }
    public string? RequestId => null;
    public string? SentId { get; set; }
    public string? ChannelType => null;
    public string? MessageType => null;
    public DateTimeOffset? SentAt => null;
    public string? SentBy => null;
    public string? SentFrom => null;
    public IDictionary<string, object?> Headers { get; } = new Dictionary<string, object?>();
    public IConfigurationSection Config { get; } = config;

    public object? this[string key]
    {
        get => Headers.TryGetValue(key, out var value) ? value : null;
        set => Headers[key] = value;
    }
}
