using Scalar.AspNetCore;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Contributes to the Scalar API reference options (for example OAuth2 sign-in) when <see cref="EndpointRouteBuilderExtensions.MapApiReference"/> runs.
/// </summary>
public interface IApiReferenceConfigurator
{
    /// <summary>
    /// Configures the Scalar API reference.
    /// </summary>
    /// <param name="options">The Scalar options to configure.</param>
    void Configure(ScalarOptions options);
}
