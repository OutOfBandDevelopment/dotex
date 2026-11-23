namespace OoBDev.Microsoft.Azure.EventHub;

/// <summary>Settings for the Azure Event Hubs client, bound from the <c>OoBDev:Azure:EventHub</c> section.</summary>
public sealed class AzureEventHubOptions
{
    /// <summary>The configuration section.</summary>
    public const string SectionName = "OoBDev:Azure:EventHub";

    /// <summary>Namespace or hub connection string.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>The hub name; may be empty when the connection string names the hub.</summary>
    public string EventHubName { get; set; } = string.Empty;

    /// <summary>The consumer group; the default group when empty.</summary>
    public string ConsumerGroup { get; set; } = string.Empty;

    /// <summary>True to read each partition from its start; false reads only new events.</summary>
    public bool ReadFromEarliest { get; set; }
}
