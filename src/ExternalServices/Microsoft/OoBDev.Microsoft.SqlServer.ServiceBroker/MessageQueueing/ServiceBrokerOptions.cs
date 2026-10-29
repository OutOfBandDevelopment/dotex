using Microsoft.Extensions.Configuration;
using OoBDev.System;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;

/// <summary>
/// The settings of one Service Broker queue, read from a message queue configuration section.
/// </summary>
public sealed partial record ServiceBrokerOptions
{
    /// <summary>
    /// The largest retry count; SQL Server disables a queue after five consecutive rollbacks, so the provider ends a poison message first.
    /// </summary>
    public const int MaximumRetries = 4;

    /// <summary>The SQL Server connection string.</summary>
    public required string ConnectionString { get; init; }

    /// <summary>The queue name (a plain identifier, optionally schema qualified).</summary>
    public required string QueueName { get; init; }

    /// <summary>The service that is bound to the queue and used as the sending service.</summary>
    public required string ServiceName { get; init; }

    /// <summary>The service that receives the messages; defaults to <see cref="ServiceName"/>.</summary>
    public required string TargetServiceName { get; init; }

    /// <summary>The contract of the dialog.</summary>
    public string ContractName { get; init; } = ServiceBrokerGlobals.DefaultContractName;

    /// <summary>How often a failing handler is retried before the conversation is ended with an error.</summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>How long one receive waits for a message before it loops again.</summary>
    public int ReceiveTimeoutSeconds { get; init; } = 5;

    /// <summary>When true the message type, contract, queue and service are created if they are missing.</summary>
    public bool EnsureObjects { get; init; }

    /// <summary>
    /// Reads the options from a configuration section.
    /// </summary>
    /// <param name="config">The section that holds the settings.</param>
    /// <returns>The validated options.</returns>
    /// <exception cref="ConfigurationMissingException">A required key is missing.</exception>
    /// <exception cref="InvalidOperationException">A value is not valid.</exception>
    public static ServiceBrokerOptions From(IConfigurationSection config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var serviceName = config[nameof(ServiceName)] ?? throw new ConfigurationMissingException($"{config.Path}:{nameof(ServiceName)}");
        var options = new ServiceBrokerOptions
        {
            ConnectionString = config[nameof(ConnectionString)] ?? throw new ConfigurationMissingException($"{config.Path}:{nameof(ConnectionString)}"),
            QueueName = config[nameof(QueueName)] ?? throw new ConfigurationMissingException($"{config.Path}:{nameof(QueueName)}"),
            ServiceName = serviceName,
            TargetServiceName = config[nameof(TargetServiceName)] ?? serviceName,
            ContractName = config[nameof(ContractName)] ?? ServiceBrokerGlobals.DefaultContractName,
            MaxRetries = ReadInt(config, nameof(MaxRetries), 3),
            ReceiveTimeoutSeconds = ReadInt(config, nameof(ReceiveTimeoutSeconds), 5),
            EnsureObjects = bool.TryParse(config[nameof(EnsureObjects)], out var ensure) && ensure,
        };

        options.Validate();
        return options;
    }

    /// <summary>
    /// Checks the values; the queue name is placed into T-SQL as an identifier, so only plain names pass.
    /// </summary>
    /// <exception cref="InvalidOperationException">A value is not valid.</exception>
    public void Validate()
    {
        if (!QueueNamePattern().IsMatch(QueueName))
        {
            throw new InvalidOperationException($"Queue name '{QueueName}' must be an identifier of letters, digits and underscores, optionally prefixed with a schema.");
        }

        if (MaxRetries is < 1 or > MaximumRetries)
        {
            throw new InvalidOperationException($"MaxRetries must be between 1 and {MaximumRetries}.");
        }

        if (ReceiveTimeoutSeconds is < 1 or > 3600)
        {
            throw new InvalidOperationException("ReceiveTimeoutSeconds must be between 1 and 3600.");
        }

        if (string.IsNullOrWhiteSpace(ServiceName) || string.IsNullOrWhiteSpace(TargetServiceName) || string.IsNullOrWhiteSpace(ContractName))
        {
            throw new InvalidOperationException("Service, target service and contract names must not be empty.");
        }
    }

    /// <summary>
    /// The queue name quoted for T-SQL, for example <c>[dbo].[orders]</c>.
    /// </summary>
    public string QuotedQueueName => string.Join('.', Array.ConvertAll(QueueName.Split('.'), part => $"[{part}]"));

    private static int ReadInt(IConfigurationSection config, string key, int fallback) =>
        int.TryParse(config[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;

    [GeneratedRegex(@"^([A-Za-z_][A-Za-z0-9_]*\.)?[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex QueueNamePattern();
}
