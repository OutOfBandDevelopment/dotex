using System;

namespace OoBDev.System.Reflection;

/// <summary>
/// Represents an interface for resolving a <see cref="Type"/>.
/// </summary>
public interface IResolveType
{
    /// <summary>
    /// Resolves and returns the associated <see cref="Type"/>.
    /// </summary>
    /// <returns>The resolved <see cref="Type"/>.</returns>
    Type ResolveType();
}
