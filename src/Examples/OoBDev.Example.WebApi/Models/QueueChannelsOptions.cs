using System;
using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>The <c>MessageQueue</c> configuration section, keyed by channel (message type) name.</summary>
public class QueueChannelsOptions : Dictionary<string, QueueChannelOptions>
{
    /// <summary>Initializes a new instance of the <see cref="QueueChannelsOptions"/> class.</summary>
    public QueueChannelsOptions() : base(StringComparer.OrdinalIgnoreCase)
    {
    }
}
