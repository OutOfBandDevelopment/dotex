using Microsoft.Extensions.Options;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.WebApi.Services;

/// <summary>Describes the configured message queue channels without exposing connection strings or other secrets.</summary>
public class QueueChannelCatalog(IOptions<QueueChannelsOptions> options)
{
    /// <summary>Lists the configured channels, ordered by name.</summary>
    /// <returns>One status per channel; only the queue name, host name and receiver flag are shown.</returns>
    public IReadOnlyList<QueueChannelStatusModel> List() =>
        [.. options.Value
            .OrderBy(c => c.Key, StringComparer.OrdinalIgnoreCase)
            .Select(c => new QueueChannelStatusModel
            {
                Channel = c.Key,
                Provider = c.Value.Provider ?? "unset",
                QueueName = Setting(c.Value, "QueueName"),
                HostName = Setting(c.Value, "HostName"),
                ReceiverEnabled = !bool.TryParse(Setting(c.Value, "DisableReceiver"), out var disabled) || !disabled,
            })];

    private static string? Setting(QueueChannelOptions channel, string key) =>
        channel.Config.TryGetValue(key, out var value) ? value : null;
}
