using Microsoft.Extensions.Configuration;
using OoBDev.System;
using System;
using System.Globalization;

namespace OoBDev.PostgreSql.Pgmq.MessageQueueing;

/// <summary>
/// The settings of one pgmq queue, read from the message queue configuration section.
/// </summary>
/// <param name="ConnectionString">The Npgsql connection string of the database that has the pgmq extension.</param>
/// <param name="QueueName">The pgmq queue name (created on first use).</param>
/// <param name="VisibilityTimeout">How long a read message stays hidden before it is delivered again when it was not archived.</param>
/// <param name="PollInterval">The wait between reads when the queue is empty.</param>
/// <param name="BatchSize">The most messages read at once.</param>
public sealed record PgmqSettings(
    string ConnectionString,
    string QueueName,
    TimeSpan VisibilityTimeout,
    TimeSpan PollInterval,
    int BatchSize)
{
    /// <summary>
    /// Reads the settings. <c>ConnectionString</c> and <c>QueueName</c> are required;
    /// <c>VisibilityTimeoutSeconds</c> (30), <c>PollIntervalMilliseconds</c> (500) and <c>BatchSize</c> (10) are optional.
    /// </summary>
    /// <param name="config">The configuration section of the queue.</param>
    /// <exception cref="ConfigurationMissingException">A required key is missing.</exception>
    public static PgmqSettings From(IConfigurationSection config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return new PgmqSettings(
            config[nameof(ConnectionString)] ?? throw new ConfigurationMissingException($"{config.Path}:{nameof(ConnectionString)}"),
            config[nameof(QueueName)] ?? throw new ConfigurationMissingException($"{config.Path}:{nameof(QueueName)}"),
            TimeSpan.FromSeconds(Number(config, "VisibilityTimeoutSeconds", 30)),
            TimeSpan.FromMilliseconds(Number(config, "PollIntervalMilliseconds", 500)),
            (int)Number(config, nameof(BatchSize), 10));
    }

    private static double Number(IConfigurationSection config, string key, double fallback)
        => double.TryParse(config[key], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : fallback;
}
