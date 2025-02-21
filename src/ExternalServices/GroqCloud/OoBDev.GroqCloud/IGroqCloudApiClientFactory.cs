
using GroqNet;

namespace OoBDev.GroqCloud;

/// <summary>
/// Represents a factory for creating instances of the GroqCloudApiClient.
/// </summary>
public interface IGroqCloudApiClientFactory
{
    /// <summary>
    /// Builds an instance of the GroqCloudApiClient for the specified host.
    /// </summary>
    /// <returns>An instance of the GroqCloudApiClient.</returns>
    GroqClient Build();
}
