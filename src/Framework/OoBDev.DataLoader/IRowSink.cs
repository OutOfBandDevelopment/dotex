using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>Stores accepted rows.</summary>
public interface IRowSink
{
    /// <summary>Stores a row, applying the duplicate policy.</summary>
    /// <param name="row">The row.</param>
    /// <param name="policy">Duplicate handling.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    ValueTask<WriteResult> WriteAsync(RowContext row, DuplicatePolicy policy, CancellationToken cancellationToken);
}
