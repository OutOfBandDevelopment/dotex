using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Search.Graph;

/// <summary>
/// Stores a property graph and answers the usual retrieval questions: who is connected within N steps, and the shortest path.
/// </summary>
public interface IGraphStore
{
    /// <summary>
    /// Creates the node, or replaces its properties when it exists.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the node is stored.</returns>
    Task UpsertNodeAsync(GraphNode node, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a directed edge between two existing nodes (once per type).
    /// </summary>
    /// <param name="from">The source node.</param>
    /// <param name="type">The edge type, for example <c>KNOWS</c>.</param>
    /// <param name="to">The target node.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the edge is stored.</returns>
    Task ConnectAsync(GraphNode from, string type, GraphNode to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the nodes reachable from a node in one to <paramref name="maxDepth"/> steps, following edges in either direction.
    /// </summary>
    /// <param name="start">The start node.</param>
    /// <param name="maxDepth">The largest number of steps.</param>
    /// <param name="edgeType">Only follow this edge type, or any when null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The distinct reachable nodes, nearest first.</returns>
    IAsyncEnumerable<GraphNode> NeighborsAsync(GraphNode start, int maxDepth, string? edgeType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the shortest path between two nodes.
    /// </summary>
    /// <param name="from">The first node.</param>
    /// <param name="to">The last node.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The nodes on the path, both ends included, or an empty list when none exists.</returns>
    Task<IReadOnlyList<GraphNode>> ShortestPathAsync(GraphNode from, GraphNode to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a node and its edges.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the node is gone.</returns>
    Task DeleteNodeAsync(GraphNode node, CancellationToken cancellationToken = default);
}
