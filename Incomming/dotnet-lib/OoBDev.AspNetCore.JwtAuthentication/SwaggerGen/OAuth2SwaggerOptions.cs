namespace OoBDev.AspNetCore.JwtAuthentication.SwaggerGen;

/// <summary>
/// Represents the options for configuring OAuth2 in Swagger.
/// </summary>
public class OAuth2SwaggerOptions
{
    /// <summary>
    /// Gets or sets the claim that Swagger will use to determine the authenticated user's API access.
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
