namespace OoBDev.AspNetCore.JwtAuthentication.OpenApi;

/// <summary>
/// The endpoints read from an OpenID configuration document.
/// </summary>
/// <param name="AuthorizationUrl">The authorization endpoint.</param>
/// <param name="TokenUrl">The token endpoint.</param>
public record OpenIdEndpoints(string AuthorizationUrl, string TokenUrl);
