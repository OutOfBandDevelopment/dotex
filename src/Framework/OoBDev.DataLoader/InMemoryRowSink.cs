using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>Stores rows by one key column in memory; for tests and small loads.</summary>
/// <param name="keyColumn">The column that identifies a row.</param>
public sealed class InMemoryRowSink(string keyColumn) : IRowSink
{
    private readonly Dictionary<string, Dictionary<string, string?>> _rows = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The stored rows by key.</summary>
    public IReadOnlyDictionary<string, Dictionary<string, string?>> Rows => _rows;

    /// <inheritdoc />
    public ValueTask<WriteResult> WriteAsync(RowContext row, DuplicatePolicy policy, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(row);
        var key = row.Values.TryGetValue(keyColumn, out var k) ? k ?? string.Empty : string.Empty;
        if (!_rows.ContainsKey(key))
        {
            _rows[key] = new Dictionary<string, string?>(row.Values, StringComparer.OrdinalIgnoreCase);
            return ValueTask.FromResult(WriteResult.Written);
        }

        switch (policy)
        {
            case DuplicatePolicy.Replace:
                _rows[key] = new Dictionary<string, string?>(row.Values, StringComparer.OrdinalIgnoreCase);
                return ValueTask.FromResult(WriteResult.Replaced);
            case DuplicatePolicy.Skip:
                return ValueTask.FromResult(WriteResult.Skipped);
            default:
                return ValueTask.FromResult(WriteResult.Duplicate);
        }
    }
}
