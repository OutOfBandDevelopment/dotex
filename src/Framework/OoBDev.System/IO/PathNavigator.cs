using OoBDev.System.Xml.XPath;
using System;
using System.IO;
using System.Xml.XPath;

namespace OoBDev.System.IO;

/// <summary>
/// Provides XPath navigation capabilities for file system directories.
/// Converts directory structures into XPath-navigable representations.
/// </summary>
public class PathNavigator : IToXPathNavigable
{
    /// <summary>
    /// Converts a file system directory path into an XPath-navigable representation.
    /// </summary>
    /// <param name="filePath">The path to the directory to navigate.</param>
    /// <returns>An IXPathNavigable object representing the directory structure.</returns>
    public IXPathNavigable ToNavigable(string filePath) => new DirectoryInfo(filePath).ToNavigable();

    /// <summary>
    /// Stream-based navigation is not supported for file system paths.
    /// </summary>
    /// <param name="stream">The stream (not supported).</param>
    /// <returns>This method always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Always thrown as streams are not supported for path navigation.</exception>
    public IXPathNavigable ToNavigable(Stream stream) => throw new NotSupportedException();
}
