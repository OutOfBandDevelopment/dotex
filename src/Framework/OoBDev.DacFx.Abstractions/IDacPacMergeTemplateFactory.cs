using System.Threading.Tasks;

namespace OoBDev.DacFx;

/// <summary>
/// Factory for creating <see cref="IDacPacMergeTemplate"/> instances.
/// </summary>
public interface IDacPacMergeTemplateFactory
{
    /// <summary>
    /// Creates a merge template asynchronously.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation, containing a configured <see cref="IDacPacMergeTemplate"/> instance.
    /// </returns>
    Task<IDacPacMergeTemplate> Create();
}