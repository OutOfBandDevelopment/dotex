using System.Threading.Tasks;

namespace OoBDev.VendorName.CapabilityName;

/// <summary>
/// Wraps the VendorName SDK so it can be mocked. Replace with the real client type.
/// </summary>
public interface IVendorNameClientFactory
{
    /// <summary>
    /// Sends a request through the vendor client.
    /// </summary>
    /// <param name="request">Request payload.</param>
    /// <returns>Vendor response.</returns>
    Task<string> SendAsync(string request);
}
