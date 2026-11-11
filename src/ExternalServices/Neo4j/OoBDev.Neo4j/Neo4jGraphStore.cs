using Microsoft.Extensions.Options;
using Neo4j.Driver;
using OoBDev.Search.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Neo4j;

/// <summary>
/// Stores a property graph in Neo4j and queries it with Cypher. Nodes are matched by their <c>id</c> property and label.
/// </summary>
public sealed partial class Neo4jGraphStore(IDriver driver, IOptions<Neo4jOptions> options) : IGraphStore
{
    private const string IdKey = "id";

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Identifier();

    /// <inheritdoc />
    public Task UpsertNodeAsync(GraphNode node, CancellationToken cancellationToken = default)
    {
        var properties = new Dictionary<string, object?>(node.Properties, StringComparer.Ordinal) { [IdKey] = node.Id };
        return RunAsync(
            $"MERGE (n:{Name(node.Label)} {{id: $id}}) SET n = $properties",
            new { id = node.Id, properties },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task ConnectAsync(GraphNode from, string type, GraphNode to, CancellationToken cancellationToken = default) =>
        RunAsync(
            $"MATCH (a:{Name(from.Label)} {{id: $from}}), (b:{Name(to.Label)} {{id: $to}}) MERGE (a)-[:{Name(type)}]->(b)",
            new { from = from.Id, to = to.Id },
            cancellationToken);

    /// <inheritdoc />
    public async IAsyncEnumerable<GraphNode> NeighborsAsync(
        GraphNode start,
        int maxDepth,
        string? edgeType = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDepth, 1);
        var relationship = edgeType is null ? "" : $":{Name(edgeType)}";
        var result = await QueryAsync(
            $"MATCH p = (s:{Name(start.Label)} {{id: $id}})-[{relationship}*1..{maxDepth}]-(n) WHERE n <> s RETURN n, min(length(p)) AS depth ORDER BY depth, n.id",
            new { id = start.Id },
            cancellationToken).ConfigureAwait(false);
        foreach (var record in result)
            yield return ToNode(record["n"].As<INode>());
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GraphNode>> ShortestPathAsync(GraphNode from, GraphNode to, CancellationToken cancellationToken = default)
    {
        if (from.Label == to.Label && from.Id == to.Id)
            return [from];
        var result = await QueryAsync(
            $"MATCH (a:{Name(from.Label)} {{id: $from}}), (b:{Name(to.Label)} {{id: $to}}) MATCH p = shortestPath((a)-[*..{options.Value.MaxPathLength}]-(b)) RETURN nodes(p) AS path",
            new { from = from.Id, to = to.Id },
            cancellationToken).ConfigureAwait(false);
        return result.Count == 0
            ? []
            : result[0]["path"].As<List<INode>>().Select(ToNode).ToList();
    }

    /// <inheritdoc />
    public Task DeleteNodeAsync(GraphNode node, CancellationToken cancellationToken = default) =>
        RunAsync($"MATCH (n:{Name(node.Label)} {{id: $id}}) DETACH DELETE n", new { id = node.Id }, cancellationToken);

    internal static string Name(string value) =>
        Identifier().IsMatch(value)
            ? value
            : throw new ArgumentException($"'{value}' is not a valid label or edge type (letters, digits and underscore, not starting with a digit).", nameof(value));

    internal static GraphNode ToNode(INode node)
    {
        var properties = node.Properties.Where(p => p.Key != IdKey).ToDictionary(p => p.Key, p => (object?)p.Value, StringComparer.Ordinal);
        return new GraphNode(node.Properties[IdKey].As<string>(), node.Labels[0], properties);
    }

    private async Task RunAsync(string cypher, object parameters, CancellationToken cancellationToken) =>
        await QueryAsync(cypher, parameters, cancellationToken).ConfigureAwait(false);

    private async Task<IReadOnlyList<IRecord>> QueryAsync(string cypher, object parameters, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var query = driver.ExecutableQuery(cypher).WithParameters(parameters);
        if (!string.IsNullOrEmpty(options.Value.Database))
            query = query.WithConfig(new QueryConfig(database: options.Value.Database));
        var result = await query.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        return result.Result;
    }
}
