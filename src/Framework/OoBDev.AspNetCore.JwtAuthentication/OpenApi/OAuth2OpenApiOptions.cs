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
    /// Gets or sets the URL for the authorization endpoint.
    /// </summary>
    public required string AuthorizationUrl { get; set; }

    /// <summary>
    /// Gets or sets the URL for the token endpoint.
    /// </summary>
    public required string TokenUrl { get; set; }
}
