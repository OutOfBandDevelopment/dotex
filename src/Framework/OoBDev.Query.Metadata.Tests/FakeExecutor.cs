using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Query.Metadata.Tests;

internal sealed class FakeExecutor : IQueryExecutor
{
    public List<(string Sql, IReadOnlyDictionary<string, object?> Parameters)> Calls { get; } = [];

    public Queue<IReadOnlyList<IReadOnlyDictionary<string, object?>>> Results { get; } = new();

    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ReadAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken)
    {
        Calls.Add((sql, parameters));
        return Task.FromResult(Results.Count > 0 ? Results.Dequeue() : []);
    }
}
