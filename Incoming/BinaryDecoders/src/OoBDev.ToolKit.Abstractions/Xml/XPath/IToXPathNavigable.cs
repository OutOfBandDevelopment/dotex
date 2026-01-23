using System.IO;
using System.Xml.XPath;

namespace OoBDev.ToolKit.Xml.XPath;

public interface IToXPathNavigable
{
    IXPathNavigable? ToNavigable(string filePath);
    IXPathNavigable? ToNavigable(Stream stream);
}
