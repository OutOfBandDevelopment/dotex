using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OoBDev.DacFx;

namespace OoBDev.DacPacCompiler.Cli;

/// <summary>
/// Hosted service that merges several DACPAC files into one package.
/// </summary>
/// <param name="compiler">The merge compiler.</param>
/// <param name="definition">The merge definition expanded from the template and the command line.</param>
public sealed class DacPacMergeEngineService(
    IDacpacMergeCompiler compiler,
    IDacPacMergeDefinition definition
    ) : IHostedService
{
    /// <summary>
    /// Merges the source DACPACs into the target path.
    /// </summary>
    /// <param name="cancellationToken">Token to signal cancellation.</param>
    /// <returns>A completed task.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        compiler.CreatePackage(definition);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the service (no-op).
    /// </summary>
    /// <param name="cancellationToken">Token to signal cancellation.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
