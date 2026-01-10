namespace OoBDev.AspNetCore.Extensions;

public record OpenApiOptions
{
    public const string OptionName = "OpenApi";

    public bool EnableSwagger { get; init; }
    public bool EnableHttpsRedirection { get; init; } = true;
}

