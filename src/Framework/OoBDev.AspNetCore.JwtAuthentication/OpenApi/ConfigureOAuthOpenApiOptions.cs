using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;

namespace OoBDev.AspNetCore.JwtAuthentication.OpenApi;

/// <summary>
/// Adds the <see cref="OAuthDocumentTransformer"/> to every OpenAPI document.
/// </summary>
public class ConfigureOAuthOpenApiOptions : IConfigureOptions<OpenApiOptions>
{
    /// <summary>
    /// Adds the OAuth2 document transformer.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    public void Configure(OpenApiOptions options) => options.AddDocumentTransformer<OAuthDocumentTransformer>();
}
