using Microsoft.Extensions.Configuration;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace OoBDev.AsyncApi;

/// <summary>
/// Builds AsyncAPI documents from the registered message handlers and the <c>MessageQueue</c> configuration.
/// </summary>
public class AsyncApiDocumentBuilder(
    IEnumerable<IMessageQueueHandler> handlers,
    IMessagePropertyResolver resolver,
    IConfiguration configuration,
    IEnumerable<IAsyncApiContributor> contributors)
{
    /// <summary>The name of the document that contains everything.</summary>
    public const string AllDocumentName = "all";

    private static readonly JsonSerializerOptions SchemaOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new JsonStringEnumConverter() },
    };

    private sealed record HandlerInfo(Type Channel, Type Message, Assembly Assembly);

    private IEnumerable<HandlerInfo> Handlers() =>
        from handler in handlers
        let type = handler.GetType()
        let iface = type.GetInterfaces()
            .Where(i => typeof(IMessageQueueHandler).IsAssignableFrom(i))
            .OrderByDescending(i => i.GenericTypeArguments.Length)
            .FirstOrDefault()
        where iface != null
        select new HandlerInfo(
            iface.GenericTypeArguments.FirstOrDefault() ?? typeof(object),
            iface.GenericTypeArguments.ElementAtOrDefault(1) ?? typeof(object),
            type.Assembly);

    /// <summary>Document names: <c>all</c> plus one per assembly that contains handlers.</summary>
    public virtual IReadOnlyList<string> DocumentNames() =>
        [AllDocumentName, .. Handlers().Select(h => h.Assembly.GetName().Name!).Distinct(StringComparer.OrdinalIgnoreCase)];

    /// <summary>Builds the named document, or returns null for an unknown name.</summary>
    public virtual AsyncApiDocument? Build(string name)
    {
        var isAll = string.Equals(name, AllDocumentName, StringComparison.OrdinalIgnoreCase);
        var selected = Handlers()
            .Where(h => isAll || string.Equals(h.Assembly.GetName().Name, name, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (!isAll && selected.Count == 0)
        {
            return null;
        }

        var document = new AsyncApiDocument
        {
            Info =
            {
                Title = configuration["AsyncApi:Title"] ?? (isAll ? "Messaging" : name),
                Version = configuration["AsyncApi:Version"] ?? "1.0.0",
            },
        };

        var covered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var handler in selected)
        {
            var (config, target, message, path) = resolver.ConfigurationSafe(handler.Channel, handler.Message);
            if (path != null)
            {
                covered.Add(ParentPath(path));
            }

            var providerKey = resolver.ProviderSafe(handler.Channel, handler.Message).providerKey;
            var channel = AddChannel(document, config, target, message, providerKey);
            channel.Messages[message] = new AsyncApiMessage { Payload = SchemaRef(document, message, handler.Message) };
            document.Operations[$"receive{Safe(target)}{Safe(message)}"] = new AsyncApiOperation
            {
                Action = "receive",
                Channel = new AsyncApiReference { Ref = $"#/channels/{target}.{message}" },
            };
        }

        if (isAll)
        {
            AddConfiguredSenders(document, covered);
        }

        return document;
    }

    // Senders are open generics (IMessageQueueSender<T>), so queues with no handler come from configuration only.
    private void AddConfiguredSenders(AsyncApiDocument document, HashSet<string> covered)
    {
        foreach (var entry in configuration.GetSection("MessageQueue").GetChildren())
        {
            var sections = entry["Provider"] != null
                ? [(section: entry, target: entry.Key, message: entry.Key)]
                : entry.GetChildren().Where(c => c["Provider"] != null).Select(c => (section: c, target: entry.Key, message: c.Key)).ToArray();

            foreach (var (section, target, message) in sections)
            {
                if (covered.Contains(section.Path) || document.Channels.ContainsKey($"{target}.{message}"))
                {
                    continue;
                }

                var config = section.GetSection("Config").GetChildren().Any() ? section.GetSection("Config") : section;
                var channel = AddChannel(document, config, target, message, section["Provider"]);
                channel.Messages[message] = new AsyncApiMessage();
                document.Operations[$"send{Safe(target)}{Safe(message)}"] = new AsyncApiOperation
                {
                    Action = "send",
                    Channel = new AsyncApiReference { Ref = $"#/channels/{target}.{message}" },
                };
            }
        }
    }

    private AsyncApiChannel AddChannel(AsyncApiDocument document, IConfigurationSection? config, string target, string message, string? providerKey)
    {
        var key = $"{target}.{message}";
        var channel = new AsyncApiChannel { Address = message };
        document.Channels[key] = channel;

        var contributor = providerKey == null
            ? null
            : contributors.FirstOrDefault(c => string.Equals(c.ProviderKey, providerKey, StringComparison.OrdinalIgnoreCase));
        var contribution = config == null ? null : contributor?.Describe(config);
        if (contribution != null)
        {
            document.Servers[contribution.ServerName] = contribution.Server;
            channel.Servers = [new AsyncApiReference { Ref = $"#/servers/{contribution.ServerName}" }];
            channel.Address = contribution.Address ?? message;
            channel.Bindings = contribution.Bindings;
        }
        else if (providerKey != null)
        {
            channel.Description = $"Provider: {providerKey}";
        }

        return channel;
    }

    private static JsonNode SchemaRef(AsyncApiDocument document, string name, Type type)
    {
        if (!document.Components.Schemas.ContainsKey(name))
        {
            document.Components.Schemas[name] = type == typeof(object)
                ? new JsonObject()
                : JsonSchemaExporter.GetJsonSchemaAsNode(SchemaOptions, type);
        }

        return new JsonObject { ["$ref"] = $"#/components/schemas/{name}" };
    }

    private static string ParentPath(string path)
    {
        var index = path.LastIndexOf(':');
        return index > 0 && path.EndsWith(":Config", StringComparison.OrdinalIgnoreCase) ? path[..index] : path;
    }

    private static string Safe(string value) => new(value.Where(char.IsLetterOrDigit).ToArray());
}
