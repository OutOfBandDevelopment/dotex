using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// Represents configuration options for SwaggerGen related to health check endpoints.
/// </summary>
public class HealthCheckSwaggerGenEndpointOptions : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Configures SwaggerGen options to use the <see cref="HealthChecksDocumentFilter"/> for filtering documents.
    /// </summary>
    /// <param name="options">The SwaggerGen options to configure.</param>
    public void Configure(SwaggerGenOptions options) => options.DocumentFilter<HealthChecksDocumentFilter>();
}
