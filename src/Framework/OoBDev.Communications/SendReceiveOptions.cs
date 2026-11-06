using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.Communications;

/// <summary>Retry settings of <see cref="SendService"/>, bound from <c>OoBDev:Communications</c>.</summary>
public sealed class SendReceiveOptions
{
    /// <summary>Gets or sets how many retries follow the first attempt.</summary>
    [Range(0, 100)]
    public int RetryCount { get; set; } = 3;

    /// <summary>Gets or sets the delay before the first retry.</summary>
    [Range(typeof(TimeSpan), "00:00:00.001", "1.00:00:00")]
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Gets or sets the factor the delay grows by with every retry.</summary>
    [Range(1.0, 100.0)]
    public double BackoffFactor { get; set; } = 2;

    /// <summary>Gets or sets the longest delay between two attempts.</summary>
    [Range(typeof(TimeSpan), "00:00:00.001", "1.00:00:00")]
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Gets or sets how far a webhook call may differ from the clock before it is rejected.</summary>
    [Range(typeof(TimeSpan), "00:00:00.001", "1.00:00:00")]
    public TimeSpan WebhookClockTolerance { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Gets or sets the configured channels.</summary>
    public IList<ChannelDefinition> Channels { get; set; } = [];
}
