using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;

namespace OoBDev.AspNetCore.Extensions.SwaggerGen;

/// <inheritdoc />
public class AdditionalSwaggerGenEndpointsOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly ILogger<AdditionalSwaggerGenEndpointsOptions> _log;

    /// <inheritdoc />
    public AdditionalSwaggerGenEndpointsOptions(
        ILogger<AdditionalSwaggerGenEndpointsOptions> log
        ) => _log = log;

    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        var commentFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
        foreach (var file in commentFiles)
        {
            try
            {
                _log.LogWarning($"Loading comments from \"{{{nameof(file)}}}\"", Path.GetFileName(file));
                options.IncludeXmlComments(file);
            }
            catch (Exception e)
            {
                _log.LogWarning($"{{{nameof(file)}}}: {{{nameof(e.Message)}}}", Path.GetFileName(file), e.Message);
            }
        }
    }
}
