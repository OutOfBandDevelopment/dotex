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
/// <param name="discovery">Reads the endpoints from the authority when they are not set.</param>
/// <param name="logger">Logger</param>
public partial class OAuthDocumentTransformer(
    IOptions<OAuth2OpenApiOptions> config,
    OpenIdConfigurationDiscovery discovery,
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
    public static IDictionary<string, string> GetScopes(OAuth2OpenApiOptions options)
    {
        var scopes = new Dictionary<string, string>(StringComparer.Ordinal);

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
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var options = config.Value;
        var authorizationUrl = options.AuthorizationUrl;
        var tokenUrl = options.TokenUrl;

        if ((string.IsNullOrEmpty(authorizationUrl) || string.IsNullOrEmpty(tokenUrl)) && !string.IsNullOrWhiteSpace(options.Authority))
        {
            var endpoints = await discovery.GetAsync(options.Authority, cancellationToken).ConfigureAwait(false);
            if (endpoints is not null)
            {
                authorizationUrl = string.IsNullOrEmpty(authorizationUrl) ? endpoints.AuthorizationUrl : authorizationUrl;
                tokenUrl = string.IsNullOrEmpty(tokenUrl) ? endpoints.TokenUrl : tokenUrl;
            }
        }

        if (string.IsNullOrEmpty(authorizationUrl))
        {
            LogAuthorizationUrlNotConfigured(logger, nameof(OAuth2OpenApiOptions));
            return;
        }
        if (string.IsNullOrEmpty(tokenUrl))
        {
            LogTokenUrlNotConfigured(logger, nameof(OAuth2OpenApiOptions));
            return;
        }

        var scopes = GetScopes(options);

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        document.Components.SecuritySchemes[SecuritySchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "oauth2 authentication (authorization code with PKCE)",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    Scopes = scopes,
                    AuthorizationUrl = new Uri(authorizationUrl),
                    TokenUrl = new Uri(tokenUrl),
                },
            },
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SecuritySchemeName, document)] = [.. scopes.Keys],
        });
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "{Section}:AuthorizationUrl is not configured and could not be read from the authority")]
    private static partial void LogAuthorizationUrlNotConfigured(ILogger logger, string section);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "{Section}:TokenUrl is not configured and could not be read from the authority")]
    private static partial void LogTokenUrlNotConfigured(ILogger logger, string section);
}
