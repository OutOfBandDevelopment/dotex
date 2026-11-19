using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Checks, compiles and runs a request, and builds the keyset cursor of the next page. Only the kind, the shape and the timing are logged, never row content or request values.
/// </summary>
/// <param name="registry">The kinds.</param>
/// <param name="dialect">The SQL dialect.</param>
/// <param name="executor">Runs the SQL.</param>
/// <param name="cursors">Signs and reads cursors.</param>
/// <param name="options">The options.</param>
/// <param name="logger">The logger.</param>
/// <param name="authorizer">Checks the right a kind names; without one such kinds answer 403.</param>
public sealed partial class QueryEngine(
    KindRegistry registry,
    IQueryDialect dialect,
    IQueryExecutor executor,
    CursorCodec cursors,
    IOptions<QueryMetadataOptions> options,
    ILogger<QueryEngine> logger,
    IQueryAuthorizer? authorizer = null)
{
    /// <summary>The kinds this engine answers for.</summary>
    public KindRegistry Registry => registry;

    /// <summary>Runs a request and returns rows as dictionaries (facet name to value), or <see cref="FacetCount"/>-shaped dictionaries for the facets shape.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One page.</returns>
    /// <exception cref="QueryException">The request is not allowed (400), the caller lacks the right (403) or the cursor is invalid (400).</exception>
    public async Task<QueryResult<IReadOnlyDictionary<string, object?>>> QueryAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var started = Stopwatch.GetTimestamp();
        var kind = registry.Get(request.From);
        if (kind.Right.Length > 0 && authorizer?.IsAllowed(kind.Right) != true)
            throw new QueryException($"Kind '{kind.Name}' needs the right '{kind.Right}'.", 403);

        if (request.Shape == "graph")
            return await GraphAsync(kind, request, started, cancellationToken).ConfigureAwait(false);

        var compiled = QueryCompiler.Compile(kind, request, dialect, options.Value.MaxTop, cursors);
        var rows = await executor.ReadAsync(compiled.Sql, compiled.Parameters, cancellationToken).ConfigureAwait(false);

        string? next = null;
        IReadOnlyList<IReadOnlyDictionary<string, object?>> page = rows;
        if (compiled.SortColumns > 0)
        {
            if (rows.Count > compiled.Top)
            {
                var last = rows[compiled.Top - 1];
                var values = Enumerable.Range(0, compiled.SortColumns).Select(i => ToJson(last["__s" + i])).ToArray();
                next = cursors.Encode(new CursorState(kind.Name, compiled.SortSpec, values));
            }

            var taken = rows.Take(compiled.Top).ToList();
            var hints = kind.Hints is { Count: > 0 } && request.Select?.Contains("has", StringComparer.OrdinalIgnoreCase) == true && taken.Count > 0
                ? await ReadHintsAsync(kind, taken, compiled.SortColumns - 1, cancellationToken).ConfigureAwait(false)
                : null;
            page = taken.Select(r => Strip(r, hints, compiled.SortColumns - 1)).ToList();
        }

        long? count = null;
        if (compiled.CountSql is not null)
        {
            var counted = await executor.ReadAsync(compiled.CountSql, compiled.Parameters, cancellationToken).ConfigureAwait(false);
            count = counted.Count > 0 ? Convert.ToInt64(counted[0].Values.First(), CultureInfo.InvariantCulture) : 0;
        }

        var elapsed = (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        Ran(logger, kind.Name, request.Shape, elapsed);
        return new QueryResult<IReadOnlyDictionary<string, object?>> { Kind = kind.Name, Shape = request.Shape, Rows = page, Next = next, Count = count, ElapsedMs = elapsed };
    }

    /// <summary>Runs a request and maps the rows to the kind's row class.</summary>
    /// <typeparam name="T">The row class of the kind (or <see cref="FacetCount"/> for the facets shape).</typeparam>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One page of typed rows.</returns>
    public async Task<QueryResult<T>> QueryAsync<T>(QueryRequest request, CancellationToken cancellationToken = default)
        where T : new()
    {
        var result = await QueryAsync(request, cancellationToken).ConfigureAwait(false);
        return new QueryResult<T>
        {
            Kind = result.Kind,
            Shape = result.Shape,
            Rows = result.Rows.Select(RowMapper.Map<T>).ToList(),
            Next = result.Next,
            Count = result.Count,
            ElapsedMs = result.ElapsedMs,
            Nodes = result.Nodes,
        };
    }

    private async Task<QueryResult<IReadOnlyDictionary<string, object?>>> GraphAsync(KindInfo kind, QueryRequest request, long started, CancellationToken cancellationToken)
    {
        var top = Math.Clamp(request.Top, 1, options.Value.MaxTop);
        var edges = new Dictionary<string, IReadOnlyDictionary<string, object?>>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var frontier = request.Root is null ? null : new[] { request.Root };
        var steps = request.Root is null ? 1 : Math.Clamp(request.Depth, 1, 4);
        for (var step = 0; step < steps && edges.Count < top; step++)
        {
            var compiled = QueryCompiler.Graph(kind, request, dialect, new SqlParameters(), top, frontier);
            var rows = await executor.ReadAsync(compiled.Sql, compiled.Parameters, cancellationToken).ConfigureAwait(false);
            var next = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in rows)
            {
                var source = KeyText(row["source"]);
                var target = KeyText(row["target"]);
                var weight = Convert.ToDouble(row["weight"] ?? 0, CultureInfo.InvariantCulture);
                if (edges.Count >= top || !edges.TryAdd(source + "\u0001" + target, new Dictionary<string, object?> { ["source"] = source, ["target"] = target, ["weight"] = weight }))
                    continue;
                foreach (var node in new[] { source, target })
                    if (seen.Add(node))
                        next.Add(node);
            }

            if (frontier is null)
                break;
            frontier = next.Except(frontier).ToArray();
            if (frontier.Length == 0)
                break;
        }

        var list = edges.Values.ToList();
        var elapsed = (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        Ran(logger, kind.Name, request.Shape, elapsed);
        return new QueryResult<IReadOnlyDictionary<string, object?>>
        {
            Kind = kind.Name,
            Shape = "graph",
            Rows = list,
            ElapsedMs = elapsed,
            Nodes = list.SelectMany(e => new[] { (string)e["source"]!, (string)e["target"]! }).Distinct(StringComparer.Ordinal).ToList(),
        };
    }

    private async Task<Dictionary<string, IReadOnlyDictionary<string, bool>>> ReadHintsAsync(KindInfo kind, List<IReadOnlyDictionary<string, object?>> rows, int keyColumn, CancellationToken cancellationToken)
    {
        var hints = kind.Hints!;
        var keys = rows.Select(r => r["__s" + keyColumn]).Where(k => k is not null).Cast<object>().ToList();
        var names = hints.Keys.ToArray();
        var expressions = names.Select(n => $"({hints[n].Replace("{k}", kind.Key, StringComparison.Ordinal)}) AS {dialect.Quote(n)}");
        var parameters = new SqlParameters();
        var sql = $"SELECT {kind.Key} AS {dialect.Quote("__key")}, {string.Join(", ", expressions)} FROM {kind.Base} {kind.Joins}".TrimEnd()
            + $" WHERE {dialect.In(kind.Key, parameters.Add(TypedArray(keys)))}";
        var found = await executor.ReadAsync(sql, parameters.Values, cancellationToken).ConfigureAwait(false);
        return found.ToDictionary(
            r => KeyText(r["__key"]),
            r => (IReadOnlyDictionary<string, bool>)names.ToDictionary(n => n, n => r.TryGetValue(n, out var v) && v is true, StringComparer.OrdinalIgnoreCase));
    }

    private static string KeyText(object? key) => Convert.ToString(key, CultureInfo.InvariantCulture) ?? "";

    private static Array TypedArray(List<object> keys) => keys[0] switch
    {
        long => keys.Cast<long>().ToArray(),
        int => keys.Cast<int>().ToArray(),
        Guid => keys.Cast<Guid>().ToArray(),
        _ => keys.Select(KeyText).ToArray(),
    };

    private static Dictionary<string, object?> Strip(IReadOnlyDictionary<string, object?> row, Dictionary<string, IReadOnlyDictionary<string, bool>>? hints, int keyColumn)
    {
        var result = row.Where(kv => !kv.Key.StartsWith("__s", StringComparison.Ordinal)).ToDictionary(kv => kv.Key, kv => kv.Value);
        if (hints is not null)
        {
            var key = row["__s" + keyColumn];
            result["has"] = hints.TryGetValue(KeyText(key), out var flags) ? flags : new Dictionary<string, bool>();
        }

        return result;
    }

    private static JsonElement ToJson(object? value) => value switch
    {
        null => JsonSerializer.SerializeToElement<object?>(null),
        DateTime dt => JsonSerializer.SerializeToElement(dt.ToString("O", CultureInfo.InvariantCulture)),
        DateTimeOffset dto => JsonSerializer.SerializeToElement(dto.UtcDateTime.ToString("O", CultureInfo.InvariantCulture)),
        _ => JsonSerializer.SerializeToElement(value, value.GetType()),
    };

    [LoggerMessage(Level = LogLevel.Debug, Message = "Query {Kind} {Shape} took {ElapsedMs} ms")]
    private static partial void Ran(ILogger logger, string kind, string shape, long elapsedMs);
}
