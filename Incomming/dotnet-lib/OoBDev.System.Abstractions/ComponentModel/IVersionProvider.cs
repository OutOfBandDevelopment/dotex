using System.Reflection;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Provides information about the version of an assembly.
/// </summary>
public interface IVersionProvider
{
    /// <summary>
    /// Gets the title of the assembly.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Gets the version of the assembly.
    /// </summary>
    string? Version { get; }

    /// <summary>
    /// Gets the description of the assembly.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the <see cref="Assembly"/> associated with the version information.
    /// </summary>
    Assembly? Assembly { get; }
}
