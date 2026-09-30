using OoBDev.System.Utilities;
using System.Threading.Tasks;

namespace OoBDev.CapabilityName;

/// <summary>
/// Default <see cref="ICapabilityNameManager"/>; delegates to the selected provider.
/// </summary>
public class CapabilityNameManager : ICapabilityNameManager
{
    private readonly ISelectedService<ICapabilityNameProvider> _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapabilityNameManager"/> class.
    /// </summary>
    /// <param name="provider">The configuration-selected provider.</param>
    public CapabilityNameManager(ISelectedService<ICapabilityNameProvider> provider)
    {
        _provider = provider;
    }

    /// <inheritdoc/>
    public Task<string> RunAsync(string request) => _provider.Value.ExecuteAsync(request);
}
