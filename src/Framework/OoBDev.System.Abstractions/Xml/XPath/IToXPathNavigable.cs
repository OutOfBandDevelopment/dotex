using System.IO;
using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Provides functionality to convert various sources into XPath-navigable objects.
/// </summary>
public interface IToXPathNavigable
{
    /// <summary>
    /// Converts a file to an XPath-navigable object.
    /// </summary>
    /// <param name="filePath">The path to the file to convert.</param>
    /// <returns>An XPath-navigable object, or null if conversion fails.</returns>
    IXPathNavigable? ToNavigable(string filePath);

    /// <summary>
    /// Converts a stream to an XPath-navigable object.
    /// </summary>
    /// <param name="stream">The stream to convert.</param>
    /// <returns>An XPath-navigable object, or null if conversion fails.</returns>
    IXPathNavigable? ToNavigable(Stream stream);
}
