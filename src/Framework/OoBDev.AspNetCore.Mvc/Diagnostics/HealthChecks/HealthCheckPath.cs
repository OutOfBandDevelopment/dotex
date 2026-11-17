namespace OoBDev.AspNetCore.Mvc.Diagnostics.HealthChecks;

/// <summary>
/// The path the health endpoint is mapped at, set by <c>UseAspNetCoreExtensionMiddleware</c> and read by the OpenAPI document transformer.
/// </summary>
public class HealthCheckPath
{
    /// <summary>The default path.</summary>
    public const string Default = "/health";

    /// <summary>Gets or sets the mapped path.</summary>
    public string Value { get; set; } = Default;
}
