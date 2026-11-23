using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>A sink that stores several rows in one round trip; the loader uses it when <see cref="LoadOptions.BatchSize"/> is above 1.</summary>
public interface IBatchRowSink : IRowSink
{
    /// <summary>Stores the rows in order, applying the duplicate policy to keys already stored and to repeats inside the batch.</summary>
    /// <param name="rows">The rows.</param>
    /// <param name="policy">Duplicate handling.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>One result per row, in the same order.</returns>
    ValueTask<IReadOnlyList<WriteResult>> WriteBatchAsync(IReadOnlyList<RowContext> rows, DuplicatePolicy policy, CancellationToken cancellationToken);
}
