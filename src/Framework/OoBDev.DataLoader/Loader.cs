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

        var run = new Run(_sink, options, cancellationToken);
        await foreach (var sourceRow in reader.ReadAsync(source, cancellationToken).ConfigureAwait(false))
        {
            run.Read++;
            var row = new RowContext(sourceRow.Line, new Dictionary<string, string?>(sourceRow.Values, StringComparer.OrdinalIgnoreCase));
            var rejected = await RunHandlersAsync(row, cancellationToken).ConfigureAwait(false);
            if (rejected is not null)
            {
                run.Errors.Add(new RowError(row.Line, rejected));
                if (options.StopOnError) break;
                continue;
            }

            if (await run.AcceptAsync(row).ConfigureAwait(false))
            {
                return run.Report();
            }
        }

        await run.FinishAsync().ConfigureAwait(false);
        return run.Report();
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

    private sealed class Run(IRowSink sink, LoadOptions options, CancellationToken cancellationToken)
    {
        private readonly List<RowContext>? _batch = sink is IBatchRowSink && options.BatchSize > 1 ? new List<RowContext>(options.BatchSize) : null;
        private int _written, _replaced, _skipped;

        public int Read { get; set; }

        public List<RowError> Errors { get; } = [];

        public LoadReport Report() => new(Read, _written, _replaced, _skipped, Errors);

        public async Task FinishAsync()
        {
            if (_batch is not null)
            {
                await FlushAsync().ConfigureAwait(false);
            }
        }

        /// <returns>True when the load must stop.</returns>
        public async Task<bool> AcceptAsync(RowContext row)
        {
            if (_batch is not null)
            {
                _batch.Add(row);
                return _batch.Count >= options.BatchSize && await FlushAsync().ConfigureAwait(false);
            }

            var result = await sink.WriteAsync(row, options.Duplicates, cancellationToken).ConfigureAwait(false);
            return Count(result, row.Line) && options.StopOnError;
        }

        private async Task<bool> FlushAsync()
        {
            var rows = _batch!;
            if (rows.Count == 0) return false;
            var results = await ((IBatchRowSink)sink).WriteBatchAsync(rows, options.Duplicates, cancellationToken).ConfigureAwait(false);
            var failed = false;
            for (var i = 0; i < rows.Count; i++)
            {
                failed |= Count(results[i], rows[i].Line);
            }

            rows.Clear();
            return failed && options.StopOnError;
        }

        /// <returns>True when the result was a duplicate error.</returns>
        private bool Count(WriteResult result, int line)
        {
            switch (result)
            {
                case WriteResult.Written: _written++; return false;
                case WriteResult.Replaced: _replaced++; return false;
                case WriteResult.Skipped: _skipped++; return false;
                default:
                    Errors.Add(new RowError(line, "Duplicate row"));
                    return true;
            }
        }
    }
}
