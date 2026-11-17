namespace OoBDev.AspNetCore.JwtAuthentication.OpenApi;

/// <summary>
/// Represents the options for configuring OAuth2 sign-in in the OpenAPI documents and the API reference.
/// </summary>
public class OAuth2OpenApiOptions
{
    /// <summary>
    /// Gets or sets the claim (scope) that the API reference requests to determine the authenticated user's API access.
    /// </summary>
    public required string UserReadApiClaim { get; set; }

    /// <summary>
    /// Gets or sets the URL for the authorization endpoint. Empty reads it from <see cref="Authority"/>.
    /// </summary>
    public string? AuthorizationUrl { get; set; }

    /// <summary>
    /// Gets or sets the URL for the token endpoint. Empty reads it from <see cref="Authority"/>.
    /// </summary>
    public string? TokenUrl { get; set; }

    /// <summary>
    /// Gets or sets the authority (issuer) URL whose <c>.well-known/openid-configuration</c> supplies the authorization and token URLs that are not set.
    /// </summary>
    public string? Authority { get; set; }

    /// <summary>
    /// Gets or sets the client id the API reference signs in with. Empty uses the JwtBearer audience.
    /// </summary>
    public string? ClientId { get; set; }
}
