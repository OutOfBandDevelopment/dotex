namespace OoBDev.WebApi.Models;

/// <summary>A configured message queue channel, without its secrets.</summary>
public class QueueChannelStatusModel
{
    /// <summary>Gets or sets the channel name.</summary>
    public required string Channel { get; set; }

    /// <summary>Gets or sets the provider key.</summary>
    public required string Provider { get; set; }

    /// <summary>Gets or sets the queue name, when the provider has one.</summary>
    public string? QueueName { get; set; }

    /// <summary>Gets or sets the broker host name, when the provider has one.</summary>
    public string? HostName { get; set; }

    /// <summary>Gets or sets a value indicating whether this host receives from the queue.</summary>
    public bool ReceiverEnabled { get; set; }
}
