using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OoBDev.AspNetCore.Extensions;

public class EnvironmentSettings : IEnvironmentSettings
{
    private readonly IOptions<OpenApiOptions> _options;
    private readonly IWebHostEnvironment? _environment;

    public EnvironmentSettings(
        IOptions<OpenApiOptions> options,
        IWebHostEnvironment? environment = default
        )
    {
        _options = options;
        _environment = environment;
    }

    public bool EnableSwagger =>
        (_environment?.IsDevelopment() ?? false) ||
        (_environment?.IsLocal() ?? false) ||
        _options.Value.EnableSwagger;

    public bool EnableHttpsRedirection =>
        _options.Value.EnableHttpsRedirection;
}

