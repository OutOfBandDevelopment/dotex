using OoBDev.System.IO;
using OoBDev.System.Security;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using static OoBDev.System.ToolkitConstants;

namespace OoBDev.System.Xml.Xsl.Extensions;

/// <summary>
/// Sandboxable wrapper around File IO functions intended for use with XslCompiledTransform
/// </summary>
/// <remarks>
/// </remarks>
/// <param name="sandbox">base path for sandboxing these functions</param>
[XmlRoot(Namespace = XmlNamespaces.Base + nameof(FileExtensions))]
public partial class FileExtensions(string sandbox, bool emptyOnUnreadable = false, ILogger? logger = null)
{
    private readonly ILogger _logger = logger ?? NullLogger.Instance;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source">input XPathNavigator to output</param>
    /// <param name="filePath">file path to write</param>
    /// <returns>source</returns>
    public XPathNavigator WriteToFile(XPathNavigator source, string filePath)
    {
        if (string.IsNullOrWhiteSpace(source.Value)) return source;
        var realPath = SandboxPath.EnsureSafePath(sandbox, filePath).CreateParentIfNotExists();
        if (realPath == null)
        {
            LogWriteSkipped(_logger, filePath);
        }
        else
        {
            File.WriteAllText(realPath, source.Value);
        }
        return source;
    }

    /// <summary>
    /// Reads an XML file inside the sandbox. A missing file throws <see cref="FileNotFoundException"/> and a file that is not XML throws <see cref="XmlException"/>,
    /// unless the extension was created with <c>emptyOnUnreadable</c>, which returns an empty document and logs a warning instead.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The document navigator.</returns>
    public XPathNavigator ReadXml(string filePath)
    {
        var realPath = SandboxPath.EnsureSafePath(sandbox, filePath) ?? throw new NotSupportedException("null path not allowed");
        try
        {
            if (!File.Exists(realPath))
                throw new FileNotFoundException("XML file not found", realPath);
            return XDocument.Load(realPath).CreateNavigator();
        }
        catch (Exception ex) when (emptyOnUnreadable && ex is FileNotFoundException or XmlException)
        {
            LogReadEmpty(_logger, ex, filePath);
            return new XDocument().CreateNavigator();
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "WriteToFile skipped: the path \"{FilePath}\" is outside the sandbox or null")]
    private static partial void LogWriteSkipped(ILogger logger, string filePath);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "ReadXml returned an empty document for \"{FilePath}\"")]
    private static partial void LogReadEmpty(ILogger logger, Exception exception, string filePath);
}
