using System;

namespace OoBDev.Communications;

/// <summary>Retry settings of <see cref="SendService"/>, bound from <c>OoBDev:Communications</c>.</summary>
public sealed class SendReceiveOptions
{
    /// <summary>Gets or sets how many retries follow the first attempt.</summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>Gets or sets the delay before the first retry.</summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Gets or sets the factor the delay grows by with every retry.</summary>
    public double BackoffFactor { get; set; } = 2;

    /// <summary>Gets or sets the longest delay between two attempts.</summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
}
