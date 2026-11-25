using OoBDev.System.Text.Templating;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Html;

/// <summary>
/// An <see cref="IInstanceFactory"/> that serves path resolvers over JSON. Any object is serialised with
/// System.Text.Json (a <see cref="JsonNode"/> is used as is). Formatters and other template transforms are not provided.
/// </summary>
public class JsonInstanceFactory : IInstanceFactory
{
    /// <inheritdoc />
    public Task<IPathResolver> GetPathResolver(object source)
    {
        JsonNode? node = source switch
        {
            null => null,
            JsonNode n => n,
            string s => JsonNode.Parse(s),
            _ => JsonSerializer.SerializeToNode(source),
        };
        return Task.FromResult<IPathResolver>(new JsonPathResolver(node));
    }

    /// <exception cref="NotSupportedException">Always; this factory only creates path resolvers.</exception>
    public Task<IFormatter> GetFormatter(object source) =>
        throw new NotSupportedException("JsonInstanceFactory creates path resolvers only.");

    /// <exception cref="NotSupportedException">Always; this factory only creates path resolvers.</exception>
    public Task<ITemplateTransform> GetTemplateTransform(string mediaType) =>
        throw new NotSupportedException("JsonInstanceFactory creates path resolvers only.");
}
