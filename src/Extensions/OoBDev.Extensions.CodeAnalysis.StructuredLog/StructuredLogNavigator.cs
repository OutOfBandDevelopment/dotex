using System.IO;
using System.Xml.XPath;
using Microsoft.Build.Logging.StructuredLogger;
using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;

namespace OoBDev.Extensions.CodeAnalysis.StructuredLog;

/// <summary>Opens an MSBuild binary log (<c>.binlog</c>) as a navigable document.</summary>
[FileExtension(".binlog")]
public class StructuredLogNavigator : IToXPathNavigable
{
    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(string filePath) => Serialization.Read(filePath).ToNavigable();

    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(Stream stream) => Serialization.ReadBinLog(stream).ToNavigable();
}
