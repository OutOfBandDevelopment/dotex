using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>Replaces or adds a column with the value found for another column; a missing key rejects the row unless the lookup is optional.</summary>
/// <param name="priority">Pipeline order.</param>
/// <param name="source">The lookup.</param>
/// <param name="keyColumn">The column holding the lookup key.</param>
/// <param name="targetColumn">The column that receives the found value.</param>
/// <param name="required">When true an empty or unknown key rejects the row; when false the row passes unchanged.</param>
public sealed class LookupHandler(int priority, ILookupSource source, string keyColumn, string targetColumn, bool required = true) : IPipelineHandler
{
    /// <inheritdoc />
    public int Priority => priority;

    /// <inheritdoc />
    public async ValueTask<RowOutcome> HandleAsync(RowContext row, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(row);
        var key = row.Values.TryGetValue(keyColumn, out var k) ? k : null;
        if (string.IsNullOrWhiteSpace(key))
        {
            return required ? RowOutcome.Reject($"'{keyColumn}' is empty") : RowOutcome.Accept;
        }

        var found = await source.FindAsync(key, cancellationToken).ConfigureAwait(false);
        if (found is null)
        {
            return required ? RowOutcome.Reject($"'{keyColumn}' value '{key}' was not found") : RowOutcome.Accept;
        }

        row.Values[targetColumn] = found;
        return RowOutcome.Accept;
    }
}
