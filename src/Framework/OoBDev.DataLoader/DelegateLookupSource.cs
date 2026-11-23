using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>A lookup backed by a query supplied by the host, for example a database or service call.</summary>
/// <param name="find">The query.</param>
public sealed class DelegateLookupSource(Func<string, CancellationToken, ValueTask<string?>> find) : ILookupSource
{
    /// <inheritdoc />
    public ValueTask<string?> FindAsync(string key, CancellationToken cancellationToken) => find(key, cancellationToken);
}
