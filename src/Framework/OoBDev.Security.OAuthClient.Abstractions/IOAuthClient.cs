using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Security.OAuthClient;

/// <summary>The OAuth 2.0 public-client flows for a desktop or console application. The library never opens a browser itself.</summary>
public interface IOAuthClient
{
    /// <summary>Authorization code with PKCE on a loopback redirect (RFC 7636, RFC 8252).</summary>
    /// <param name="openBrowser">Called once with the address the person must visit.</param>
    /// <param name="cancellationToken">Cancels the sign-in.</param>
    /// <returns>The tokens, also written to the secret store.</returns>
    Task<OAuthTokens> SignInWithBrowserAsync(Action<Uri> openBrowser, CancellationToken cancellationToken = default);

    /// <summary>Device authorization grant (RFC 8628) for clients with no browser.</summary>
    /// <param name="showPrompt">Called once with the code and address to show the person.</param>
    /// <param name="cancellationToken">Cancels the sign-in.</param>
    /// <returns>The tokens, also written to the secret store.</returns>
    Task<OAuthTokens> SignInWithDeviceCodeAsync(Action<DeviceCodePrompt> showPrompt, CancellationToken cancellationToken = default);

    /// <summary>Exchanges a refresh token for new tokens and stores them.</summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The new tokens.</returns>
    /// <exception cref="OAuthException">The provider refused the refresh token (<c>invalid_grant</c>); the stored tokens are cleared.</exception>
    Task<OAuthTokens> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Clears the stored tokens and revokes the refresh token when the provider offers revocation.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>A task that ends when the sign-out is done.</returns>
    Task SignOutAsync(CancellationToken cancellationToken = default);
}
