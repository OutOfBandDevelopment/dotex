using System;
using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>One entry of the <c>MessageQueue</c> configuration section: the provider key and its settings.</summary>
public class QueueChannelOptions
{
    /// <summary>Gets or sets the provider key (for example <c>rabbit-mq</c>).</summary>
    public string? Provider { get; set; }

    /// <summary>Gets or sets the provider settings.</summary>
    public Dictionary<string, string?> Config { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
