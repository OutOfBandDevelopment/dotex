using System;

namespace OoBDev.MessageQueueing.Hosting;

/// <summary>Settings for <see cref="OutboxDispatcherHost"/>.</summary>
public class OutboxDispatcherHostOptions
{
    /// <summary>How long to wait between runs when the outbox is empty or a send failed.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>The most entries sent per run.</summary>
    public int BatchSize { get; set; } = 100;
}
