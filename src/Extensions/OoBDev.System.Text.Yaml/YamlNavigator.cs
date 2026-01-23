using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using YamlDotNet.RepresentationModel;

namespace OoBDev.System.Text.Yaml;

/// <summary>
/// Converts YAML documents to XPath-navigable representations, supporting multiple YAML file extensions and media types.
/// Implements <see cref="IToXPathNavigable"/> to enable YAML documents to be queried using XPath expressions.
/// </summary>
[FileExtension(".yaml")]
[FileExtension(".yml")]
[MediaType("text/yaml")]
[MediaType("text/vnd.yaml")]
[MediaType("text/x-yaml")]
[MediaType("application/yaml")]
[MediaType("application/vnd.yaml")]
[MediaType("application/x-yaml")]
public class YamlNavigator : IToXPathNavigable
{
    /// <summary>
    /// Converts a YAML file to an XPath-navigable representation.
    /// </summary>
    /// <param name="filePath">The path to the YAML file to load and convert.</param>
    /// <returns>An IXPathNavigable instance representing the YAML document, or null if the document cannot be loaded.</returns>
    public IXPathNavigable? ToNavigable(string filePath)
    {
        using var input = new StreamReader(filePath);
        return ToNavigable(input);
    }

    /// <summary>
    /// Converts a YAML stream to an XPath-navigable representation.
    /// </summary>
    /// <param name="stream">The stream containing YAML content to load and convert.</param>
    /// <returns>An IXPathNavigable instance representing the YAML document, or null if the document cannot be loaded.</returns>
    public IXPathNavigable? ToNavigable(Stream stream)
    {
        using var input = new StreamReader(stream);
        return ToNavigable(input);
    }

    private IXPathNavigable? ToNavigable(StreamReader reader)
    {
        var yaml = new YamlStream();
        yaml.Load(reader);
        return yaml.Documents.SingleOrDefault()?.ToNavigable();
    }
}
