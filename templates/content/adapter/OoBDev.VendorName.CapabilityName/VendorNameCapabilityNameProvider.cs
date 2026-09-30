using System.Threading.Tasks;
using OoBDev.CapabilityName;

namespace OoBDev.VendorName.CapabilityName;

/// <summary>
/// VendorName implementation of <see cref="ICapabilityNameProvider"/>.
/// </summary>
public class VendorNameCapabilityNameProvider : ICapabilityNameProvider
{
    private readonly IVendorNameClientFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="VendorNameCapabilityNameProvider"/> class.
    /// </summary>
    /// <param name="factory">Vendor client factory.</param>
    public VendorNameCapabilityNameProvider(IVendorNameClientFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public Task<string> ExecuteAsync(string request) => _factory.SendAsync(request);
}
