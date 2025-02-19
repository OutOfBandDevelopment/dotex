using System;

namespace OoBDev.System.Providers;

/// <summary>
/// Represents a provider for generating and handling GUIDs.
/// </summary>
public class GuidProvider : IGuidProvider
{

    /// <summary>
    /// Gets an empty GUID.
    /// </summary>
    /// <remarks>
    /// This property returns a GUID with all bits set to zero.
    /// </remarks>
    public Guid Empty => Guid.Empty;

    /// <summary>
    /// Generates a new GUID.
    /// </summary>
    /// <returns>A new GUID.</returns>
    public Guid NewGuid() => Guid.NewGuid();
}
