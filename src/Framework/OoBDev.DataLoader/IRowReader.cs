using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>Reads rows from a stream in one format.</summary>
public interface IRowReader
{
    /// <summary>The format name, such as <c>csv</c> or <c>json</c>.</summary>
    string Format { get; }

    /// <summary>Reads the rows.</summary>
    /// <param name="source">The input.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    IAsyncEnumerable<SourceRow> ReadAsync(Stream source, CancellationToken cancellationToken);
}
