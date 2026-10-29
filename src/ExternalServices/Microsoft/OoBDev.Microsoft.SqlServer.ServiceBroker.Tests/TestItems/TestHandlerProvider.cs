using Microsoft.Extensions.Configuration;
using OoBDev.MessageQueueing.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.Tests.TestItems;

public sealed class TestHandlerProvider(IConfigurationSection config, int failFirst = 0) : IMessageHandlerProvider
{
    private int _calls;

    public ConcurrentQueue<string> Received { get; } = new();

    public int Calls => _calls;

    public IConfigurationSection Config { get; } = config;

    public Task HandleAsync(IQueueMessage message, string messageId)
    {
        var call = global::System.Threading.Interlocked.Increment(ref _calls);
        if (call <= failFirst)
        {
            throw new InvalidOperationException($"planned failure {call}");
        }

        Received.Enqueue(message.Payload?.ToString() ?? "");
        return Task.CompletedTask;
    }
}
