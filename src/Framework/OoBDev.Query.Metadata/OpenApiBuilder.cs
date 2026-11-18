using System.Linq;
using System.Text.Json.Nodes;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Builds the OpenAPI 3.1 document of the query surface from the registry, so a new kind appears in the contract without editing it. Facet metadata travels as <c>x-</c> extensions.
/// </summary>
public static class OpenApiBuilder
{
    /// <summary>Builds the document.</summary>
    /// <param name="registry">The registry.</param>
    /// <param name="title">The document title.</param>
    /// <param name="version">The document version.</param>
    /// <returns>The OpenAPI document.</returns>
    public static JsonObject Build(KindRegistry registry, string title, string version)
    {
        var schemas = new JsonObject
        {
            ["QueryRequest"] = RequestSchema(registry),
            ["Problem"] = Obj(("title", Str()), ("status", Int()), ("detail", Str())),
        };
        foreach (var kind in registry.Kinds)
        {
            schemas[kind.RowType.Name] = RowSchema(kind);
            schemas[kind.RowType.Name + "Page"] = Obj(
                ("kind", Str()), ("shape", Str()), ("rows", new JsonObject { ["type"] = "array", ["items"] = Ref(kind.RowType.Name) }), ("next", Str()), ("count", Int()), ("elapsedMs", Int()));
        }

        return new JsonObject
        {
            ["openapi"] = "3.1.0",
            ["info"] = new JsonObject { ["title"] = title, ["version"] = version },
            ["paths"] = new JsonObject
            {
                ["/q"] = new JsonObject { ["post"] = Operation("query", "Ask for rows or facet counts of one kind.", Ref("QueryRequest")) },
                ["/describe"] = new JsonObject { ["get"] = Operation("describe", "The kinds, facets, predicates and operators a client may use.", null) },
            },
            ["components"] = new JsonObject { ["schemas"] = schemas },
        };
    }

    private static JsonObject RequestSchema(KindRegistry registry) => Obj(
        ("from", new JsonObject { ["type"] = "string", ["enum"] = new JsonArray(registry.Kinds.Select(k => (JsonNode)k.Name).ToArray()) }),
        ("search", Str()),
        ("filters", new JsonObject { ["type"] = "object", ["additionalProperties"] = true }),
        ("sort", new JsonObject { ["type"] = "array", ["items"] = Str() }),
        ("select", new JsonObject { ["type"] = "array", ["items"] = Str() }),
        ("top", Int()),
        ("after", Str()),
        ("shape", new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("rows", "facets", "buckets") }),
        ("by", new JsonObject { ["type"] = "array", ["items"] = Str() }),
        ("count", new JsonObject { ["type"] = "boolean" }));

    private static JsonObject RowSchema(KindInfo kind)
    {
        var properties = new JsonObject();
        foreach (var f in kind.Facets)
        {
            properties[f.Name] = new JsonObject
            {
                ["type"] = f.Type,
                ["x-label"] = f.Label,
                ["x-format"] = f.Format,
                ["x-role"] = f.Role,
                ["x-filterable"] = f.Filterable,
                ["x-sortable"] = f.Sortable,
                ["x-search"] = f.Search,
                ["x-hidden"] = f.Hidden,
            };
        }

        return new JsonObject { ["type"] = "object", ["properties"] = properties, ["x-kind"] = kind.Name, ["x-title"] = kind.Title, ["x-default-sort"] = kind.DefaultSort };
    }

    private static JsonObject Operation(string id, string summary, JsonNode? body)
    {
        var operation = new JsonObject { ["operationId"] = id, ["summary"] = summary };
        if (body is not null)
            operation["requestBody"] = new JsonObject { ["required"] = true, ["content"] = new JsonObject { ["application/json"] = new JsonObject { ["schema"] = body } } };
        operation["responses"] = new JsonObject
        {
            ["200"] = new JsonObject { ["description"] = "One page." },
            ["400"] = new JsonObject
            {
                ["description"] = "The query is not valid for the kind.",
                ["content"] = new JsonObject { ["application/problem+json"] = new JsonObject { ["schema"] = Ref("Problem") } },
            },
        };
        return operation;
    }

    private static JsonObject Ref(string name) => new() { ["$ref"] = "#/components/schemas/" + name };

    private static JsonObject Str() => new() { ["type"] = "string" };

    private static JsonObject Int() => new() { ["type"] = "integer" };

    private static JsonObject Obj(params (string Name, JsonNode Schema)[] properties)
    {
        var result = new JsonObject();
        foreach (var (name, schema) in properties)
            result[name] = schema;
        return new JsonObject { ["type"] = "object", ["properties"] = result };
    }
}
