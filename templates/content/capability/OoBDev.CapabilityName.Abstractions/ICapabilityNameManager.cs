using System.Threading.Tasks;

namespace OoBDev.CapabilityName;

/// <summary>
/// Entry point for CapabilityName. Application code depends on this interface.
/// </summary>
public interface ICapabilityNameManager
{
    /// <summary>
    /// Runs the operation through the selected provider.
    /// </summary>
    /// <param name="request">Request payload.</param>
    /// <returns>Result from the selected provider.</returns>
    Task<string> RunAsync(string request);
}
