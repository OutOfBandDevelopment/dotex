using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace OoBDev.AspNetCore.Cgi.OpenApi;

/// <summary>
/// Adds <see cref="CgiOpenApiDocumentTransformer"/> to the documents named in <see cref="CgiOpenApiOptions.Documents"/>.
/// </summary>
/// <param name="settings">The CGI OpenAPI options.</param>
public class ConfigureCgiOpenApiOptions(IOptions<CgiOpenApiOptions> settings) : IConfigureNamedOptions<OpenApiOptions>
{
    /// <inheritdoc />
    public void Configure(OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Configure(options.DocumentName, options);
    }

    /// <inheritdoc />
    public void Configure(string? name, OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var documentName = name ?? options.DocumentName;
        if (settings.Value.Documents.Any(d => string.Equals(d, documentName, StringComparison.OrdinalIgnoreCase)))
            options.AddDocumentTransformer<CgiOpenApiDocumentTransformer>();
    }
}
