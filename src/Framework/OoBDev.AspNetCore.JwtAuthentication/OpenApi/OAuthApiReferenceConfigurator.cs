using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Mvc.OpenApi;
using Scalar.AspNetCore;

namespace OoBDev.AspNetCore.JwtAuthentication.OpenApi;

/// <summary>
/// Configures the Scalar API reference for OAuth2 sign-in (authorization code with PKCE) using the JwtBearer audience as the client id.
/// </summary>
/// <param name="jwt">The JwtBearer options.</param>
/// <param name="oauth">The OAuth2 options.</param>
/// <param name="logger">The logger.</param>
public class OAuthApiReferenceConfigurator(
    IOptions<JwtBearerOptions> jwt,
    IOptions<OAuth2OpenApiOptions> oauth,
    ILogger<OAuthApiReferenceConfigurator> logger
    ) : IApiReferenceConfigurator
{
    /// <summary>
    /// Configures the Scalar OAuth2 flow.
    /// </summary>
    /// <param name="options">The Scalar options to configure.</param>
    public void Configure(ScalarOptions options)
    {
        if (string.IsNullOrWhiteSpace(jwt.Value.Audience))
        {
            logger.LogWarning("JwtBearerOptions:Audience is not configured");
            return;
        }

        var scopes = OAuthDocumentTransformer.GetScopes(oauth.Value).Keys;
        options
            .AddPreferredSecuritySchemes(OAuthDocumentTransformer.SecuritySchemeName)
            .AddAuthorizationCodeFlow(OAuthDocumentTransformer.SecuritySchemeName, flow =>
            {
                flow.ClientId = jwt.Value.Audience;
                flow.Pkce = Pkce.Sha256;
                flow.SelectedScopes = [.. scopes];
            });
    }
}
