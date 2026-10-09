using OoBDev.System.DependencyInjection;
using System.Threading.Tasks;

namespace OoBDev.CapabilityName;

/// <summary>
/// Pluggable provider for CapabilityName. Vendor adapters implement this interface.
/// </summary>
[ContractConfig(
    AllowDefault = true,
    ConfigKey = "OoBDev:CapabilityNameProvider:Type"
    )]
public interface ICapabilityNameProvider
{
    /// <summary>
    /// Executes the provider operation.
    /// </summary>
    /// <param name="request">Request payload.</param>
    /// <returns>Provider result.</returns>
    Task<string> ExecuteAsync(string request);
}
