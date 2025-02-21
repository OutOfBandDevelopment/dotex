using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Json;
using OoBDev.System.Text.Xml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Provides template processing using XSLT (eXtensible Stylesheet Language Transformations).
/// </summary>
public class XsltTemplateProvider : ITemplateProvider
{
    private readonly IXmlSerializer _xmlSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="XsltTemplateProvider"/> class with the specified XML serializer.
    /// </summary>
    /// <param name="xmlSerializer">The XML serializer to be used by the provider.</param>
    public XsltTemplateProvider(
        IXmlSerializer xmlSerializer
        ) => _xmlSerializer = xmlSerializer;

    /// <summary>
    /// Gets the collection of supported content types by the template provider.
    /// 
    /// application/xslt+xml
    /// </summary>
    public virtual IReadOnlyCollection<string> SupportedContentTypes { get; } = new[]
    {
         ContentTypesExtensions.Application.XSLT,
    };

    /// <summary>
    /// Determines whether this template provider can apply template processing to the given context.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <returns><c>true</c> if the template processing can be applied; otherwise, <c>false</c>.</returns>
    public virtual bool CanApply(ITemplateContext context) =>
        SupportedContentTypes.Any(type => string.Equals(context.TemplateContentType, type, StringComparison.InvariantCultureIgnoreCase));

    /// <summary>
    /// Applies the XSLT template associated with the specified context, using the provided data,
    /// and writes the result to the target stream asynchronously.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <param name="target">The stream where the result will be written.</param>
    /// <returns>A task representing the asynchronous operation, indicating whether the application was successful.</returns>
    public virtual async Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target)
    {
        using var readStream = context.OpenTemplate(context);

        var xslt = new XslCompiledTransform(false);
        using var styleSheet = XmlReader.Create(readStream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            ConformanceLevel = ConformanceLevel.Document,
            NameTable = new NameTable(),
        });
        var xsltSettings = new XsltSettings(false, false);
        xslt.Load(styleSheet);

        var navigator = await GetNavigatorAsync(data);

        using var writer = XmlWriter.Create(target, new XmlWriterSettings
        {
            CloseOutput = false,
            ConformanceLevel = ConformanceLevel.Auto,
        });
        xslt.Transform(navigator, writer);
        return true;
    }

    private async Task<IXPathNavigable> GetNavigatorAsync(object data)
    {
        if (data is IXPathNavigable navigable)
        {
            return navigable;
        }
        else if (data is JsonDocument jsonDocument)
        {
            data = JsonNode.Parse(jsonDocument.RootElement.ToString()!)!;
        }
        else if (data is JsonElement jsonElement)
        {
            data = JsonNode.Parse(jsonElement.ToString()!)!;
        }
        else if (data is XmlDocument xmlDocument)
        {
            return xmlDocument.CreateNavigator() ?? throw new NotSupportedException("xmlDocument.CreateNavigator()");
        }
        else if (data is XDocument XDocument)
        {
            return XDocument.CreateNavigator();
        }

        if (data is JsonNode jsonNode)
        {
            var built = jsonNode.ToXFragment();
            var builtNavigator = built?.CreateNavigator();
            if (builtNavigator != null)
                return builtNavigator;
        }

        using var xml = new MemoryStream();
        await _xmlSerializer.SerializeAsync(data, data!.GetType(), xml);
        xml.Position = 0;
        var document = await XDocument.LoadAsync(xml, LoadOptions.None, CancellationToken.None);
        var navigator = document.CreateNavigator();
        return navigator;
    }
}
