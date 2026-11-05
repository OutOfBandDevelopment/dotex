using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace OoBDev.Documents.Guides;

/// <summary>Renders an API guide (markdown) from an OpenAPI or AsyncAPI document, deterministically: same document, same bytes.</summary>
public static class ApiGuideRenderer
{
    private static readonly string[] _httpMethods = ["get", "put", "post", "delete", "options", "head", "patch"];

    /// <summary>One section per operation, paths sorted ordinally, then methods in HTTP order.</summary>
    public static string OpenApi(string json, string title)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var text = new StringBuilder();
        text.Append("# ").Append(title).Append("\n\n");
        text.Append("Generated from the application's OpenAPI document by `ApiGuideTests`; do not edit by hand.\n\n");
        if (root.TryGetProperty("info", out var info))
        {
            text.Append("**Document:** ").Append(Str(info, "title")).Append(' ').Append(Str(info, "version")).Append("\n\n");
        }

        if (!root.TryGetProperty("paths", out var paths))
        {
            return text.ToString();
        }

        text.Append("## Operations\n\n");
        foreach (var path in paths.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            foreach (var method in _httpMethods.Where(m => path.Value.TryGetProperty(m, out _)))
            {
                var operation = path.Value.GetProperty(method);
                text.Append("### ").Append(method.ToUpperInvariant()).Append(" `").Append(path.Name).Append("`\n\n");
                var summary = Str(operation, "summary");
                if (summary.Length > 0)
                {
                    text.Append(summary).Append("\n\n");
                }

                AppendParameters(text, operation);
                if (operation.TryGetProperty("requestBody", out var body) && body.TryGetProperty("content", out var content))
                {
                    text.Append("Request body: ").Append(string.Join(", ", content.EnumerateObject().Select(c => $"`{c.Name}`"))).Append("\n\n");
                }

                if (operation.TryGetProperty("responses", out var responses))
                {
                    text.Append("Responses:\n\n");
                    foreach (var response in responses.EnumerateObject().OrderBy(r => r.Name, StringComparer.Ordinal))
                    {
                        var description = Str(response.Value, "description");
                        var types = response.Value.TryGetProperty("content", out var rc) ? string.Join(", ", rc.EnumerateObject().Select(c => $"`{c.Name}`")) : string.Empty;
                        text.Append("- `").Append(response.Name).Append('`');
                        if (description.Length > 0)
                        {
                            text.Append(' ').Append(description);
                        }

                        if (types.Length > 0)
                        {
                            text.Append(" (").Append(types).Append(')');
                        }

                        text.Append('\n');
                    }

                    text.Append('\n');
                }
            }
        }

        return text.ToString();
    }

    /// <summary>Channels with their address, description and messages, then the operations and the message payloads.</summary>
    public static string AsyncApi(string json, string title)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var text = new StringBuilder();
        text.Append("# ").Append(title).Append("\n\n");
        text.Append("Generated from the application's AsyncAPI document by `ApiGuideTests`; do not edit by hand.\n\n");
        if (root.TryGetProperty("info", out var info))
        {
            text.Append("**Document:** ").Append(Str(info, "title")).Append(' ').Append(Str(info, "version")).Append("\n\n");
            var description = Str(info, "description");
            if (description.Length > 0)
            {
                text.Append(description).Append("\n\n");
            }
        }

        if (root.TryGetProperty("channels", out var channels))
        {
            text.Append("## Channels\n\n");
            foreach (var channel in channels.EnumerateObject().OrderBy(c => c.Name, StringComparer.Ordinal))
            {
                text.Append("### ").Append(channel.Name).Append(" `").Append(Str(channel.Value, "address")).Append("`\n\n");
                var description = Str(channel.Value, "description");
                if (description.Length > 0)
                {
                    text.Append(description).Append("\n\n");
                }

                if (channel.Value.TryGetProperty("parameters", out var parameters))
                {
                    foreach (var parameter in parameters.EnumerateObject())
                    {
                        text.Append("- Parameter `").Append(parameter.Name).Append("`: ").Append(Str(parameter.Value, "description")).Append('\n');
                    }

                    text.Append('\n');
                }

                if (channel.Value.TryGetProperty("messages", out var messages))
                {
                    text.Append("Messages: ").Append(string.Join(", ", messages.EnumerateObject().Select(m => $"`{m.Name}`"))).Append("\n\n");
                }
            }
        }

        if (root.TryGetProperty("operations", out var operations))
        {
            text.Append("## Operations\n\n");
            foreach (var operation in operations.EnumerateObject().OrderBy(o => o.Name, StringComparer.Ordinal))
            {
                var channelRef = operation.Value.TryGetProperty("channel", out var channel) ? Str(channel, "$ref") : string.Empty;
                text.Append("- `").Append(operation.Name).Append("`: ").Append(Str(operation.Value, "action")).Append(" on `").Append(channelRef[(channelRef.LastIndexOf('/') + 1)..]).Append("`\n");
            }

            text.Append('\n');
        }

        if (root.TryGetProperty("components", out var components) && components.TryGetProperty("messages", out var componentMessages))
        {
            text.Append("## Messages\n\n");
            foreach (var message in componentMessages.EnumerateObject().OrderBy(m => m.Name, StringComparer.Ordinal))
            {
                text.Append("### ").Append(message.Name).Append("\n\n");
                var name = Str(message.Value, "name");
                var contentType = Str(message.Value, "contentType");
                text.Append("- Name: `").Append(name.Length > 0 ? name : message.Name).Append("`\n");
                if (contentType.Length > 0)
                {
                    text.Append("- Content type: `").Append(contentType).Append("`\n");
                }

                if (message.Value.TryGetProperty("payload", out var payload))
                {
                    text.Append("- Payload: ").Append(Payload(payload)).Append('\n');
                }

                text.Append('\n');
            }
        }

        return text.ToString();
    }

    /// <summary>Writes <paramref name="content"/> (LF endings) only when it differs from the file, so reruns leave git alone; true when written.</summary>
    public static bool WriteIfChanged(string path, string content)
    {
        var normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (File.Exists(path) && File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal) == normalized)
        {
            return false;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, normalized, new UTF8Encoding(false));
        return true;
    }

    private static void AppendParameters(StringBuilder text, JsonElement operation)
    {
        if (!operation.TryGetProperty("parameters", out var parameters) || parameters.GetArrayLength() == 0)
        {
            return;
        }

        text.Append("Parameters:\n\n");
        foreach (var parameter in parameters.EnumerateArray())
        {
            var type = parameter.TryGetProperty("schema", out var schema) ? Str(schema, "type") : string.Empty;
            var required = parameter.TryGetProperty("required", out var r) && r.ValueKind == JsonValueKind.True ? ", required" : string.Empty;
            text.Append("- `").Append(Str(parameter, "name")).Append("` (").Append(Str(parameter, "in")).Append(type.Length > 0 ? ", " + type : string.Empty).Append(required).Append(")\n");
        }

        text.Append('\n');
    }

    private static string Payload(JsonElement payload)
    {
        var type = Str(payload, "type");
        if (type == "object" && payload.TryGetProperty("properties", out var properties))
        {
            return "object with " + string.Join(", ", properties.EnumerateObject().Select(p => $"`{p.Name}` ({Str(p.Value, "type")})"));
        }

        return type.Length > 0 ? type : "any";
    }

    private static string Str(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()!.Trim().ReplaceLineEndings(" ")
            : string.Empty;
}
