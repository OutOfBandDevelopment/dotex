using System;

namespace OoBDev.MessageQueueing.Hosting;

/// <summary>
/// Options for <see cref="MessageReceiverHost"/>.
/// </summary>
public class MessageReceiverHostOptions
{
    /// <summary>
    /// How long the host waits before restarting a receiver that failed. Default ten seconds.
    /// </summary>
    public TimeSpan RestartDelay { get; set; } = TimeSpan.FromSeconds(10);
}
