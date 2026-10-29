using NuGet.Versioning;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.PackMan.Cli;

/// <summary>Looks up the published versions of a package.</summary>
public interface IPackageVersionSource
{
    /// <summary>Gets every published version of a package.</summary>
    /// <param name="packageName">The package id.</param>
    /// <param name="cancellationToken">Cancels the lookup.</param>
    /// <returns>The versions in ascending order.</returns>
    Task<IReadOnlyList<NuGetVersion>> GetVersionsAsync(string packageName, CancellationToken cancellationToken);
}
