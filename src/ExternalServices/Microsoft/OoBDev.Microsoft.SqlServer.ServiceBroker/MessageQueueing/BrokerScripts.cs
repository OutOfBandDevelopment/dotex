namespace OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;

/// <summary>
/// Builds the T-SQL the provider runs. Names are escaped here; nothing user supplied is concatenated unescaped.
/// </summary>
public static class BrokerScripts
{
    /// <summary>
    /// Checks that Service Broker is enabled for the current database.
    /// </summary>
    public const string BrokerEnabledQuery = "SELECT is_broker_enabled FROM sys.databases WHERE database_id = DB_ID();";

    /// <summary>
    /// Sends one message on a new dialog and ends the sending side. Parameters: <c>@from</c>, <c>@to</c>, <c>@contract</c>, <c>@body</c>.
    /// </summary>
    public static string Send { get; } =
        $"""
        DECLARE @h uniqueidentifier;
        BEGIN DIALOG CONVERSATION @h FROM SERVICE @from TO SERVICE @to ON CONTRACT @contract WITH ENCRYPTION = OFF;
        SEND ON CONVERSATION @h MESSAGE TYPE {Bracket(ServiceBrokerGlobals.MessageTypeName)} (@body);
        END CONVERSATION @h;
        """;

    /// <summary>
    /// Receives one message from the queue, waiting up to <c>@timeout</c> milliseconds.
    /// </summary>
    /// <param name="options">The queue settings.</param>
    /// <returns>The T-SQL batch.</returns>
    public static string Receive(ServiceBrokerOptions options) =>
        $"""
        WAITFOR (
            RECEIVE TOP (1) queuing_order, conversation_handle, message_type_name, message_body
            FROM {options.QuotedQueueName}
        ), TIMEOUT @timeout;
        """;

    /// <summary>
    /// Creates the message type, contract, queue and services when they do not exist. Safe to run repeatedly.
    /// </summary>
    /// <param name="options">The queue settings.</param>
    /// <returns>The T-SQL batch.</returns>
    public static string Ensure(ServiceBrokerOptions options)
    {
        var messageType = ServiceBrokerGlobals.MessageTypeName;
        var queue = options.QuotedQueueName;

        var script = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.service_message_types WHERE name = {Literal(messageType)})
                CREATE MESSAGE TYPE {Bracket(messageType)} VALIDATION = NONE;
            IF NOT EXISTS (SELECT 1 FROM sys.service_contracts WHERE name = {Literal(options.ContractName)})
                CREATE CONTRACT {Bracket(options.ContractName)} ({Bracket(messageType)} SENT BY INITIATOR);
            IF OBJECT_ID({Literal(options.QueueName)}, N'SQ') IS NULL
                CREATE QUEUE {queue};
            {EnsureService(options.ServiceName, queue, options.ContractName)}
            """;

        if (!string.Equals(options.ServiceName, options.TargetServiceName, global::System.StringComparison.Ordinal))
        {
            script += "\n" + EnsureService(options.TargetServiceName, queue, options.ContractName);
        }

        return script;
    }

    private static string EnsureService(string service, string queue, string contract) =>
        $"""
        IF NOT EXISTS (SELECT 1 FROM sys.services WHERE name = {Literal(service)})
            CREATE SERVICE {Bracket(service)} ON QUEUE {queue} ({Bracket(contract)});
        """;

    internal static string Bracket(string name) => "[" + name.Replace("]", "]]", global::System.StringComparison.Ordinal) + "]";

    internal static string Literal(string value) => "N'" + value.Replace("'", "''", global::System.StringComparison.Ordinal) + "'";
}
