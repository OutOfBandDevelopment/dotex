using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>One step of the load pipeline (validate, map, look up).</summary>
public interface IPipelineHandler
{
    /// <summary>Handlers run in ascending order.</summary>
    int Priority { get; }

    /// <summary>Handles a row; reject it to end its pipeline.</summary>
    /// <param name="row">The row, which may be changed.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    ValueTask<RowOutcome> HandleAsync(RowContext row, CancellationToken cancellationToken);
}
