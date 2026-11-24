using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>Replaces a column that names a text file with the content of the file (the source <c>GetTextData</c> handler).</summary>
/// <param name="priority">The pipeline priority.</param>
/// <param name="files">Resolves the file names against the data folder.</param>
/// <param name="columns">File-name column to the column that receives the text.</param>
/// <param name="required">Whether a named file that is missing rejects the row.</param>
public sealed class TextFileHandler(int priority, SourceFileResolver files, IReadOnlyDictionary<string, string> columns, bool required = true) : IPipelineHandler
{
    /// <inheritdoc />
    public int Priority => priority;

    /// <inheritdoc />
    public async ValueTask<RowOutcome> HandleAsync(RowContext row, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(row);
        foreach (var (fileColumn, target) in columns)
        {
            if (!row.Values.TryGetValue(fileColumn, out var name) || string.IsNullOrWhiteSpace(name))
            {
                row.Values.Remove(fileColumn);
                continue;
            }

            if (!files.TryResolve(name, out var path, out var error))
            {
                if (required)
                {
                    return RowOutcome.Reject($"'{fileColumn}': {error}");
                }

                row.Values.Remove(fileColumn);
                continue;
            }

            row.Values[target] = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            row.Values.Remove(fileColumn);
        }

        return RowOutcome.Accept;
    }
}
