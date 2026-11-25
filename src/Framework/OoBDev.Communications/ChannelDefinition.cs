using System;
using System.ComponentModel.DataAnnotations;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>A channel declared in configuration under <c>OoBDev:Communications:Channels</c>.</summary>
public sealed class ChannelDefinition : IChannel
{
    /// <inheritdoc />
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    [Required]
    public string Protocol { get; set; } = string.Empty;

    /// <inheritdoc />
    [Required]
    public string Provider { get; set; } = string.Empty;

    /// <summary>Gets or sets how often <c>ReceivePollingHost</c> polls the channel; <c>null</c> means the channel is not polled.</summary>
    public TimeSpan? PollInterval { get; set; }

    /// <summary>Gets or sets the most messages taken per poll.</summary>
    [Range(1, 1000)]
    public int PollBatchSize { get; set; } = 10;
}
