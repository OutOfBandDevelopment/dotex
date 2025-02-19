using OoBDev.AspNetCore.JwtAuthentication.SwaggerGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OoBDev.AspNetCore.JwtAuthentication;

/// <summary>
/// Represents a builder for configuring JWT extensions.
/// </summary>
public record JwtExtensionBuilder
{
    /// <summary>
    /// Gets or sets the default authentication schema for JWT.
    /// </summary>
    /// <remarks>
    /// Specifies the default authentication schema used for JWT. The default value is <see cref="JwtBearerDefaults.AuthenticationScheme"/>.
    /// </remarks>
    public string DefaultSchema { get; init; } = JwtBearerDefaults.AuthenticationScheme;

    /// <summary>
    /// Gets or sets the configuration section name for JwtBearerOptions.
    /// </summary>
    /// <remarks>
    /// Specifies the configuration section name for JwtBearerOptions. The default value is the name of <see cref="JwtBearerOptions"/>.
    /// </remarks>
    public string JwtBearerConfigurationSection { get; init; } = nameof(JwtBearerOptions);

    /// <summary>
    /// Gets or sets the configuration section name for OAuth2SwaggerOptions.
    /// </summary>
    /// <remarks>
    /// Specifies the configuration section name for OAuth2SwaggerOptions. The default value is the name of <see cref="OAuth2SwaggerOptions"/>.
    /// </remarks>
    public string OAuth2SwaggerConfigurationSection { get; init; } = nameof(OAuth2SwaggerOptions);
}
