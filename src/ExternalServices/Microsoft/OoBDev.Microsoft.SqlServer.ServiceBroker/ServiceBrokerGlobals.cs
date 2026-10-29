namespace OoBDev.Microsoft.SqlServer.ServiceBroker;

/// <summary>
/// Contains global constants related to SQL Server Service Broker.
/// </summary>
public static class ServiceBrokerGlobals
{
    /// <summary>
    /// The key associated with the Service Broker message provider.
    /// </summary>
    public const string MessageProviderKey = "sql-service-broker";

    /// <summary>
    /// The message type used for payloads sent by the provider.
    /// </summary>
    public const string MessageTypeName = "//OoBDev/MessageQueueing/Message";

    /// <summary>
    /// The contract used for dialogs created by the provider.
    /// </summary>
    public const string DefaultContractName = "//OoBDev/MessageQueueing/Contract";
}
