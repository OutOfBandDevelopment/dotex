using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace OoBDev.AspNetCore.JwtAuthentication.SwaggerGen;

/// <summary>
/// Configures SwaggerUI options for OAuth authentication.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ConfigureOAuthSwaggerUIOptions"/> class.
/// </remarks>
/// <param name="options">The JwtBearer options.</param>
/// <param name="logger">The logger options.</param>
public class ConfigureOAuthSwaggerUIOptions(
    IOptions<JwtBearerOptions> options,
    ILogger<ConfigureOAuthSwaggerUIOptions> logger
    ) : IConfigureOptions<SwaggerUIOptions>
{
    private readonly IOptions<JwtBearerOptions> _options = options;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Configures SwaggerUI options for OAuth authentication.
    /// </summary>
    /// <param name="options">The SwaggerUI options to configure.</param>
    public void Configure(SwaggerUIOptions options)
    {
        if (string.IsNullOrWhiteSpace(_options.Value.Audience))
        {
            _logger.LogWarning($"JwtBearerOptions:Audience is not configured");
            return;
        }

        options.OAuthClientId(_options.Value.Audience);
        options.OAuthUsePkce();
        options.OAuthScopeSeparator(" ");
    }
}
