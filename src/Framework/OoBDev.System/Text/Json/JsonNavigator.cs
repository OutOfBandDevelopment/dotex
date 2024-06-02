using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;
using System.IO;
using System.Text.Json;
using System.Xml.XPath;

namespace OoBDev.System.Text.Json;

[FileExtension(".json")]
[MediaType("application/json")]
public class JsonNavigator : IToXPathNavigable
{
    public IXPathNavigable ToNavigable(string inputFile)
    {
        using var file = File.OpenRead(inputFile);
        return ToNavigable(file);
    }

    public IXPathNavigable ToNavigable(Stream inputFile) =>
        JsonDocument.Parse(inputFile).ToNavigable();
}
