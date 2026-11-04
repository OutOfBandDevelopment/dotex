namespace OoBDev.DevTerm;

/// <summary>Which way a <see cref="RoutingRule"/> maps a message.</summary>
public enum RoutingDirection
{
    /// <summary>A line the device sent matches <see cref="RoutingRule.Match"/>; its payload is published to <see cref="RoutingRule.Topic"/>.</summary>
    DeviceToBroker,

    /// <summary>A broker message on <see cref="RoutingRule.Topic"/> matches <see cref="RoutingRule.Match"/>; <see cref="RoutingRule.Send"/> goes to the device.</summary>
    BrokerToDevice,
}
