namespace OoBDev.Common.AspNetCore;

/// <summary>
/// Represents a builder for configuring middleware extensions.
/// </summary>
public record MiddlewareExtensionBuilder
{
    /// <summary>
    /// Gets or initializes the path for health checks.
    /// </summary>
    public string? HealthCheckPath { get; init; } = "/health";
}
