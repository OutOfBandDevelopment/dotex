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
}
