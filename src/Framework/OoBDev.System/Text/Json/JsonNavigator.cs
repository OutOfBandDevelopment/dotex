using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;
using System.IO;
using System.Text.Json;
using System.Xml.XPath;

namespace OoBDev.System.Text.Json;

[FileExtension(".json")]
[MediaType("application/json")]
/// <summary>
/// Provides navigation capabilities for JSON documents, converting them to XPath-navigable structures.
/// </summary>
public class JsonNavigator : IToXPathNavigable
{
    /// <summary>
    /// Converts a JSON file to an XPath-navigable structure.
    /// </summary>
    /// <param name="inputFile">The path to the JSON file to convert.</param>
    /// <returns>An XPath-navigable representation of the JSON document.</returns>
    public IXPathNavigable ToNavigable(string inputFile)
    {
        using var file = File.OpenRead(inputFile);
        return ToNavigable(file);
    }

    /// <summary>
    /// Converts a JSON stream to an XPath-navigable structure.
    /// </summary>
    /// <param name="inputFile">The stream containing the JSON data to convert.</param>
    /// <returns>An XPath-navigable representation of the JSON document.</returns>
    public IXPathNavigable ToNavigable(Stream inputFile) =>
        JsonDocument.Parse(inputFile).ToNavigable();
}
