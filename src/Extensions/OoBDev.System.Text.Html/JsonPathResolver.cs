using OoBDev.System.Text.Templating;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Html;

/// <summary>
/// An <see cref="IPathResolver"/> over a <see cref="JsonNode"/>. <c>$</c> and <c>@</c> both address the node
/// the resolver was created for; the visitor routes <c>$</c> paths to the root resolver.
/// </summary>
/// <param name="node">The JSON node this resolver navigates.</param>
public class JsonPathResolver(JsonNode? node) : IPathResolver
{
    /// <summary>The node this resolver navigates.</summary>
    public JsonNode? Node { get; } = node;

    /// <summary>
    /// Selects the nodes matching <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path expression; empty selects the node itself.</param>
    /// <returns>An <c>IReadOnlyList&lt;JsonNode?&gt;</c> of the matches (empty when nothing matches).</returns>
    public Task<object> ItemSelector(string path) =>
        Task.FromResult<object>(JsonPathQuery.Select(Node, Node, path) ?? (IReadOnlyList<JsonNode?>)[]);

    /// <inheritdoc />
    public Task<bool> Test(string rule) => Task.FromResult(JsonPathQuery.Test(Node, Node, rule));
}
