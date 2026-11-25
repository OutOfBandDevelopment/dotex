using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>Runs a load.</summary>
public interface ILoader
{
    /// <summary>Reads, pipelines and writes every row; a bad row is reported, never thrown.</summary>
    /// <param name="source">The input.</param>
    /// <param name="format">The reader format name.</param>
    /// <param name="options">Load options.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task<LoadReport> LoadAsync(Stream source, string format, LoadOptions options, CancellationToken cancellationToken);
}
