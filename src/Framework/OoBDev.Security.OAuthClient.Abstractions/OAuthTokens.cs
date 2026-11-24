using System;

namespace OoBDev.Security.OAuthClient;

/// <summary>The tokens a sign-in or a refresh returned.</summary>
/// <param name="AccessToken">The bearer token for the resource server.</param>
/// <param name="RefreshToken">The refresh token, when the provider issued one (scope <c>offline_access</c>).</param>
/// <param name="ExpiresAt">When the access token stops being valid.</param>
/// <param name="IdToken">The OpenID Connect identity token, when issued.</param>
/// <param name="Scope">The scope the provider granted.</param>
public sealed record OAuthTokens(string AccessToken, string? RefreshToken, DateTimeOffset ExpiresAt, string? IdToken = null, string? Scope = null);
