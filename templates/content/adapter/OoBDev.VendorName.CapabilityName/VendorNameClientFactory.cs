using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace OoBDev.VendorName.CapabilityName;

/// <summary>
/// Default <see cref="IVendorNameClientFactory"/>. TODO: call the vendor SDK.
/// </summary>
public class VendorNameClientFactory : IVendorNameClientFactory
{
    private readonly IOptions<VendorNameOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="VendorNameClientFactory"/> class.
    /// </summary>
    /// <param name="options">Vendor options.</param>
    public VendorNameClientFactory(IOptions<VendorNameOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public Task<string> SendAsync(string request) =>
        throw new NotImplementedException($"Wire the VendorName SDK for account '{_options.Value.AccountId}'.");
}
