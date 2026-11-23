using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader;

/// <summary>Resolves a value by an alternative key, for example a natural key to a surrogate id.</summary>
public interface ILookupSource
{
    /// <summary>Finds the value for a key.</summary>
    /// <param name="key">The key from the row.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The value, or null when the key is unknown.</returns>
    ValueTask<string?> FindAsync(string key, CancellationToken cancellationToken);
}
