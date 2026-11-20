using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>Reads rows, runs the handlers in priority order and writes accepted rows to the sink.</summary>
/// <param name="readers">The available readers.</param>
/// <param name="handlers">The pipeline handlers.</param>
/// <param name="sink">Where rows go.</param>
public sealed class Loader(IEnumerable<IRowReader> readers, IEnumerable<IPipelineHandler> handlers, IRowSink sink) : ILoader
{
    private readonly IRowReader[] _readers = [.. readers];
    private readonly IPipelineHandler[] _handlers = [.. handlers.OrderBy(h => h.Priority)];
    private readonly IRowSink _sink = sink ?? throw new ArgumentNullException(nameof(sink));

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">No reader handles <paramref name="format"/>.</exception>
    public async Task<LoadReport> LoadAsync(Stream source, string format, LoadOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options);
        var reader = _readers.FirstOrDefault(r => string.Equals(r.Format, format, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotSupportedException($"No reader for format \"{format}\"");

        int read = 0, written = 0, replaced = 0, skipped = 0;
        var errors = new List<RowError>();
        await foreach (var source_row in reader.ReadAsync(source, cancellationToken).ConfigureAwait(false))
        {
            read++;
            var row = new RowContext(source_row.Line, new Dictionary<string, string?>(source_row.Values, StringComparer.OrdinalIgnoreCase));
            var rejected = await RunHandlersAsync(row, cancellationToken).ConfigureAwait(false);
            if (rejected is not null)
            {
                errors.Add(new RowError(row.Line, rejected));
                if (options.StopOnError) break;
                continue;
            }

            switch (await _sink.WriteAsync(row, options.Duplicates, cancellationToken).ConfigureAwait(false))
            {
                case WriteResult.Written: written++; break;
                case WriteResult.Replaced: replaced++; break;
                case WriteResult.Skipped: skipped++; break;
                default:
                    errors.Add(new RowError(row.Line, "Duplicate row"));
                    if (options.StopOnError) return new LoadReport(read, written, replaced, skipped, errors);
                    break;
            }
        }

        return new LoadReport(read, written, replaced, skipped, errors);
    }

    private async Task<string?> RunHandlersAsync(RowContext row, CancellationToken cancellationToken)
    {
        foreach (var handler in _handlers)
        {
            var outcome = await handler.HandleAsync(row, cancellationToken).ConfigureAwait(false);
            if (!outcome.Accepted) return outcome.Error ?? "Rejected";
        }

        return null;
    }
}
