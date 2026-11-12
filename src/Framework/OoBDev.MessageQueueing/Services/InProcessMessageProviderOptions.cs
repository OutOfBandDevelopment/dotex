using System;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Options for <see cref="InProcessMessageProvider"/>.
/// </summary>
public class InProcessMessageProviderOptions
{
    /// <summary>
    /// How long the receiver waits before polling again when the queue is empty. Default one second.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);
}
