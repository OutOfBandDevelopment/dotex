using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.JwtAuthentication.OpenApi;

/// <summary>
/// Document transformer that declares the <c>oauth2</c> authorization code security scheme (used with PKCE) and requires it for the document.
/// </summary>
/// <param name="config">The OAuth2 options.</param>
/// <param name="logger">Logger</param>
public class OAuthDocumentTransformer(
    IOptions<OAuth2OpenApiOptions> config,
    ILogger<OAuthDocumentTransformer> logger
    ) : IOpenApiDocumentTransformer
{
    /// <summary>
    /// The security scheme name in the OpenAPI document.
    /// </summary>
    public const string SecuritySchemeName = "oauth2";

    /// <summary>
    /// Gets the OAuth2 scopes.
    /// </summary>
    /// <param name="options">The OAuth2 options.</param>
    /// <returns>The scopes and their descriptions.</returns>
    public static Dictionary<string, string> GetScopes(OAuth2OpenApiOptions options)
    {
        var scopes = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(options.UserReadApiClaim))
        {
            scopes.Add(options.UserReadApiClaim, nameof(options.UserReadApiClaim));
        }

        return scopes;
    }

    /// <summary>
    /// Adds the OAuth2 security scheme and requirement to the document.
    /// </summary>
    /// <param name="document">The document to modify.</param>
    /// <param name="context">The transformer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        //TODO: consider getting from oauth ./.well-known/openid-configuration if not set
        var options = config.Value;

        if (string.IsNullOrEmpty(options.AuthorizationUrl))
        {
            logger.LogWarning("{Section}:AuthorizationUrl is not configured", nameof(OAuth2OpenApiOptions));
            return Task.CompletedTask;
        }
        if (string.IsNullOrEmpty(options.TokenUrl))
        {
            logger.LogWarning("{Section}:TokenUrl is not configured", nameof(OAuth2OpenApiOptions));
            return Task.CompletedTask;
        }

        var scopes = GetScopes(options);

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SecuritySchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "oauth2 authentication (authorization code with PKCE)",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    Scopes = scopes,
                    AuthorizationUrl = new Uri(options.AuthorizationUrl),
                    TokenUrl = new Uri(options.TokenUrl),
                },
            },
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SecuritySchemeName, document)] = [.. scopes.Keys],
        });

        return Task.CompletedTask;
    }
}
