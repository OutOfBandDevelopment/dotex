using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;
using System.IO;
using System.Text.Json;
using System.Xml.XPath;

namespace OoBDev.System.Text.Json;

/// <summary>
/// Provides navigation capabilities for JSON documents, converting them to XPath-navigable structures.
/// </summary>
[FileExtension(".json")]
[MediaType("application/json")]
public class JsonNavigator : IToXPathNavigable
{
    /// <summary>
    /// Converts a JSON file to an XPath-navigable structure.
    /// </summary>
    /// <param name="filePath">The path to the JSON file to convert.</param>
    /// <returns>An XPath-navigable representation of the JSON document.</returns>
    public IXPathNavigable ToNavigable(string filePath)
    {
        using var file = File.OpenRead(filePath);
        return ToNavigable(file);
    }

    /// <summary>
    /// Converts a JSON stream to an XPath-navigable structure.
    /// </summary>
    /// <param name="filePath">The stream containing the JSON data to convert.</param>
    /// <returns>An XPath-navigable representation of the JSON document.</returns>
    public IXPathNavigable ToNavigable(Stream filePath) =>
        JsonDocument.Parse(filePath).ToNavigable();
}
