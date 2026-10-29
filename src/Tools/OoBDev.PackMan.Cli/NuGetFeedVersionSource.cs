using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.PackMan.Cli;

/// <summary>Reads package versions from a NuGet feed.</summary>
/// <param name="nugetUrl">The feed's service index URL.</param>
/// <param name="feedType">The feed protocol.</param>
/// <param name="nugetLogger">Receives NuGet client messages.</param>
public sealed class NuGetFeedVersionSource(string nugetUrl, FeedType feedType, NuGet.Common.ILogger nugetLogger) : IPackageVersionSource, global::System.IDisposable
{
    private readonly SourceCacheContext _cache = new();
    private FindPackageByIdResource? _resource;

    /// <inheritdoc />
    public async Task<IReadOnlyList<NuGetVersion>> GetVersionsAsync(string packageName, CancellationToken cancellationToken)
    {
        var resource = _resource;
        if (resource is null)
        {
            resource = await Repository.Factory.GetCoreV3(nugetUrl, feedType)
                .GetResourceAsync<FindPackageByIdResource>(cancellationToken)
                .ConfigureAwait(false)
                ?? throw new global::System.NotSupportedException($"{nugetUrl} does not offer a package-version service");
            _resource = resource;
        }

        var versions = await resource.GetAllVersionsAsync(packageName, _cache, nugetLogger, cancellationToken).ConfigureAwait(false);
        return [.. versions.OrderBy(v => v)];
    }

    /// <inheritdoc />
    public void Dispose() => _cache.Dispose();
}
