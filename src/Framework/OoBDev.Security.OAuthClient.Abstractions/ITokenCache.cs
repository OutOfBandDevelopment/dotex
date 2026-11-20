using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Security.OAuthClient;

/// <summary>Hands out a valid access token, refreshing it when it is about to expire.</summary>
public interface ITokenCache
{
    /// <summary>Gets an access token. Callers that arrive while a refresh runs share that one refresh.</summary>
    /// <param name="rejectedToken">An access token the resource server just refused (a 401). When it is still the cached one, it is refreshed even though it looks valid.</param>
    /// <param name="cancellationToken">Cancels the wait.</param>
    /// <returns>The token, or null when nobody is signed in or the refresh token was refused (sign in again).</returns>
    Task<string?> GetAccessTokenAsync(string? rejectedToken = null, CancellationToken cancellationToken = default);
}
