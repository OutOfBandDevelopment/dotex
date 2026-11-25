using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>A lookup over a fixed table, matched by ordinal comparison unless a comparer is given.</summary>
/// <param name="table">Key to value.</param>
/// <param name="comparer">The key comparer.</param>
public sealed class DictionaryLookupSource(IReadOnlyDictionary<string, string> table, StringComparer? comparer = null) : ILookupSource
{
    private readonly Dictionary<string, string> _table = new(table, comparer ?? StringComparer.Ordinal);

    /// <inheritdoc />
    public ValueTask<string?> FindAsync(string key, CancellationToken cancellationToken)
        => ValueTask.FromResult(_table.TryGetValue(key, out var value) ? value : null);
}
