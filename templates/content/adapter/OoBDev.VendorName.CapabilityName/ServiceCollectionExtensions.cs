using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.VendorName.CapabilityName;

/// <summary>
/// Registration for the VendorName CapabilityName adapter.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the adapter when its configuration section contains an account id.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="section">Configuration section name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddVendorNameCapabilityNameServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string section = nameof(VendorNameOptions))
        => new VendorNameCapabilityNameRegistrar().AddServices(services, configuration, section);
}
