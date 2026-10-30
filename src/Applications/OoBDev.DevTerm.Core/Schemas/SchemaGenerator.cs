using System;
using System.Collections.Generic;
using global::System.Text.Json;
using global::System.Text.Json.Nodes;
using global::System.Text.Json.Schema;
using global::System.Text.Json.Serialization.Metadata;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Schemas;

/// <summary>Exports a JSON Schema per hand-written format from the C# model itself, so a schema cannot drift from the code.</summary>
public static class SchemaGenerator
{
    /// <summary>Identifier prefix (<c>oobdev://{application}/{namespace}/</c>) for each schema; the <c>$id</c> is this plus the file name and is a name, not a fetchable location.</summary>
    public const string BaseUrl = "oobdev://devterm/schemas/";

    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true, TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    /// <summary>File name (e.g. <c>device-manifest.schema.json</c>) to schema text.</summary>
    public static IReadOnlyDictionary<string, string> Generate() => new SortedDictionary<string, string>(StringComparer.Ordinal)
    {
        ["device-manifest.schema.json"] = For<DeviceManifest>("device-manifest.schema.json"),
        ["ui-definition.schema.json"] = For<UiDefinition>("ui-definition.schema.json"),
        ["connection-profile.schema.json"] = For<CliOptions>("connection-profile.schema.json", omit: [nameof(CliOptions.Password)]),
    };

    private static string For<T>(string fileName, string[]? omit = null)
    {
        var schema = JsonSchemaExporter.GetJsonSchemaAsNode(_options, typeof(T));
        if (schema is JsonObject obj)
        {
            obj["$schema"] = "https://json-schema.org/draft/2020-12/schema";
            obj["$id"] = BaseUrl + fileName;
            foreach (var name in omit ?? [])
            {
                (obj["properties"] as JsonObject)?.Remove(name);
            }
        }

        return schema.ToJsonString(_options).ReplaceLineEndings("\n") + "\n";
    }
}
