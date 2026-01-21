using System.Collections.Generic;
using System.Xml.Linq;

namespace OoBDev.System.Xml.Linq;

/// <summary>
/// Provides extension methods for creating XFragment instances.
/// </summary>
public static class XFragmentEx
{
    /// <summary>
    /// Converts a collection of XNode objects to an XFragment.
    /// </summary>
    /// <param name="nodes">The nodes to convert to an XFragment.</param>
    /// <returns>An XFragment containing the specified nodes.</returns>
    public static XFragment ToXFragment(this IEnumerable<XNode> nodes) => new(nodes);
}
