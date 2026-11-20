using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Security.OAuthClient;

/// <summary>Keeps small secrets (the signed-in tokens) for the current user.</summary>
public interface ISecretStore
{
    /// <summary>Reads a secret.</summary>
    /// <param name="name">The secret name.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The value, or null when none is stored.</returns>
    Task<string?> ReadAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Writes or replaces a secret.</summary>
    /// <param name="name">The secret name.</param>
    /// <param name="value">The value.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    /// <returns>A task that ends when the value is stored.</returns>
    Task WriteAsync(string name, string value, CancellationToken cancellationToken = default);

    /// <summary>Removes a secret; a missing one is not an error.</summary>
    /// <param name="name">The secret name.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>A task that ends when the value is gone.</returns>
    Task DeleteAsync(string name, CancellationToken cancellationToken = default);
}
