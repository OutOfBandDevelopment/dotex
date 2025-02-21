using OoBDev.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace OoBDev.AspNetCore.JwtAuthentication.SwaggerGen;

/// <summary>
/// Configures SwaggerGen options for OAuth2 authentication.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ConfigureOAuthSwaggerGenOptions"/> class.
/// </remarks>
/// <param name="config">The OAuth2 Swagger options.</param>
/// <param name="logger">Logger</param>
public class ConfigureOAuthSwaggerGenOptions(
    IOptions<OAuth2SwaggerOptions> config,
    ILogger<ConfigureOAuthSwaggerGenOptions> logger
    ) : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IOptions<OAuth2SwaggerOptions> _config = config;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Gets the OAuth2 scopes.
    /// </summary>
    /// <returns>The OAuth2 scopes.</returns>
    private Dictionary<string, string> GetScopes()
    {
        var scopes = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(_config.Value.UserReadApiClaim))
        {
            scopes.Add(_config.Value.UserReadApiClaim, nameof(_config.Value.UserReadApiClaim));
        }

        return scopes;
    }

    /// <summary>
    /// Configures SwaggerGen options for OAuth2 authentication.
    /// </summary>
    /// <param name="options">The SwaggerGen options to configure.</param>
    public void Configure(SwaggerGenOptions options)
    {
        //TODO: consider getting from oauth ./.well-known/openid-configuration if not set

        if (string.IsNullOrEmpty(_config.Value.AuthorizationUrl))
        {
            _logger.LogWarning($"ConfigureOAuthSwaggerGenOptions:AuthorizationUrl is not configured");
            return;
        }
        if (string.IsNullOrEmpty(_config.Value.TokenUrl))
        {
            _logger.LogWarning($"ConfigureOAuthSwaggerGenOptions:TokenUrl is not configured");
            return;
        }

        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Name = "oauth2",
            Description = "oauth2 authentication",

            Flows = new OpenApiOAuthFlows()
            {
                Implicit = new OpenApiOAuthFlow()
                {
                    Scopes = GetScopes(),
                    AuthorizationUrl = new Uri(_config.Value.AuthorizationUrl ??
                        throw new ConfigurationMissingException($"{nameof(OAuth2SwaggerOptions)}:{nameof(_config.Value.AuthorizationUrl)}")),
                    TokenUrl = new Uri(_config.Value.TokenUrl ??
                        throw new ConfigurationMissingException($"{nameof(OAuth2SwaggerOptions)}:{nameof(_config.Value.TokenUrl)}")),
                },
            }
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                },
                Array.Empty<string>()
            },
        });
    }
}
