using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.CapabilityName;

namespace OoBDev.VendorName.CapabilityName;

internal class VendorNameCapabilityNameRegistrar
{
    public IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration, string section)
    {
        // config-gated: do nothing when the adapter is not configured
        if (configuration.GetSection(section)?[nameof(VendorNameOptions.AccountId)] == null) return services;

        services.Configure<VendorNameOptions>(o => configuration.Bind(section, o));
        services.TryAddTransient<IVendorNameClientFactory, VendorNameClientFactory>();
        services.TryAddTransient<ICapabilityNameProvider, VendorNameCapabilityNameProvider>();                        // default
        services.TryAddKeyedTransient<ICapabilityNameProvider, VendorNameCapabilityNameProvider>(VendorNameGlobals.ProviderKey); // selectable
        return services;
    }
}
