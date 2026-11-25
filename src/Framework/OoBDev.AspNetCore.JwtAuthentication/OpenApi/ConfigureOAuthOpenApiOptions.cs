using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;

namespace OoBDev.AspNetCore.JwtAuthentication.OpenApi;

/// <summary>
/// Adds the <see cref="OAuthDocumentTransformer"/> to every OpenAPI document.
/// </summary>
/// <remarks>Named options: the documents are named (<c>all</c> and one per assembly), and a plain <see cref="IConfigureOptions{TOptions}"/> only runs for the unnamed instance.</remarks>
public class ConfigureOAuthOpenApiOptions : IConfigureNamedOptions<OpenApiOptions>
{
    /// <summary>
    /// Adds the OAuth2 document transformer to the unnamed options.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    public void Configure(OpenApiOptions options) => Configure(Options.DefaultName, options);

    /// <summary>
    /// Adds the OAuth2 document transformer to the options of the named document.
    /// </summary>
    /// <param name="name">The document name.</param>
    /// <param name="options">The OpenAPI options to configure.</param>
    public void Configure(string? name, OpenApiOptions options) => options.AddDocumentTransformer<OAuthDocumentTransformer>();
}
