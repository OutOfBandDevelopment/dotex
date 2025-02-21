using System;

namespace OoBDev.System.Providers;

/// <summary>
/// Represents a provider for generating and handling GUIDs.
/// </summary>
public interface IGuidProvider
{
    /// <summary>
    /// Generates a new GUID.
    /// </summary>
    /// <returns>A new GUID.</returns>
    Guid NewGuid();

    /// <summary>
    /// Gets an empty GUID.
    /// </summary>
    /// <remarks>
    /// This property returns a GUID with all bits set to zero.
    /// </remarks>
    Guid Empty { get; }
}
