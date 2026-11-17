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

            page = rows.Take(compiled.Top).Select(Strip).ToList();
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
        };
    }

    private static Dictionary<string, object?> Strip(IReadOnlyDictionary<string, object?> row) =>
        row.Where(kv => !kv.Key.StartsWith("__s", StringComparison.Ordinal)).ToDictionary(kv => kv.Key, kv => kv.Value);

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
