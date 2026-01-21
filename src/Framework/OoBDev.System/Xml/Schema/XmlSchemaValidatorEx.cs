using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace OoBDev.System.Xml.Schema;

/// <summary>
/// Provides XML schema validation functionality with multiple initialization options for loading schemas from various sources.
/// Supports validation against one or more XML schemas with detailed error, warning, and result reporting.
/// </summary>
public class XmlSchemaValidatorEx
{
    /// <summary>
    /// Gets the XML schema set containing all loaded schemas used for validation.
    /// </summary>
    public XmlSchemaSet XmlSchemaSet { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidatorEx"/> class with an empty schema set.
    /// </summary>
    public XmlSchemaValidatorEx()
    {
        XmlSchemaSet = new XmlSchemaSet();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidatorEx"/> class with a single schema loaded from a URI.
    /// </summary>
    /// <param name="targetNamespace">The target namespace of the schema, or null for no namespace.</param>
    /// <param name="xsdUri">The URI of the XSD schema file to load.</param>
    public XmlSchemaValidatorEx(string targetNamespace, string xsdUri)
        : this()
    {
        XmlSchemaSet.Add(targetNamespace ?? "", xsdUri);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidatorEx"/> class with a single schema loaded from an XmlReader.
    /// </summary>
    /// <param name="targetNamespace">The target namespace of the schema, or null for no namespace.</param>
    /// <param name="xmlReader">The XmlReader containing the XSD schema to load.</param>
    public XmlSchemaValidatorEx(string targetNamespace, XmlReader xmlReader)
        : this()
    {
        XmlSchemaSet.Add(targetNamespace ?? "", xmlReader);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidatorEx"/> class with a single schema loaded from an XNode.
    /// </summary>
    /// <param name="targetNamespace">The target namespace of the schema, or null for no namespace.</param>
    /// <param name="xsd">The XNode containing the XSD schema to load.</param>
    public XmlSchemaValidatorEx(string targetNamespace, XNode xsd)
        : this(targetNamespace ?? "", xsd.CreateReader())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidatorEx"/> class with multiple schemas loaded from URIs with explicit namespaces.
    /// </summary>
    /// <param name="xsdUris">A collection of namespace and XSD URI pairs to load.</param>
    public XmlSchemaValidatorEx(IEnumerable<KeyValuePair<string, string>> xsdUris)
        : this()
    {
        foreach (var xsdUri in xsdUris.Where(v => v.Value != null))
        {
            XmlSchemaSet.Add(xsdUri.Key ?? "", xsdUri.Value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidatorEx"/> class with multiple schemas loaded from XmlReaders with explicit namespaces.
    /// </summary>
    /// <param name="xsdReaders">A collection of namespace and XmlReader pairs containing XSD schemas to load.</param>
    public XmlSchemaValidatorEx(IEnumerable<KeyValuePair<string, XmlReader>> xsdReaders)
        : this()
    {
        foreach (var xsdUri in xsdReaders.Where(v => v.Value != null))
        {
            XmlSchemaSet.Add(xsdUri.Key ?? "", xsdUri.Value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidatorEx"/> class with multiple schemas loaded from XNodes with explicit namespaces.
    /// </summary>
    /// <param name="xsds">A collection of namespace and XNode pairs containing XSD schemas to load.</param>
    public XmlSchemaValidatorEx(IEnumerable<KeyValuePair<string, XNode>> xsds)
        : this()
    {
        foreach (var xsdUri in xsds.Where(v => v.Value != null))
        {
            XmlSchemaSet.Add(xsdUri.Key ?? "", xsdUri.Value.CreateReader());
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidatorEx"/> class with multiple schemas loaded from URIs.
    /// Target namespaces are automatically extracted from each schema document.
    /// </summary>
    /// <param name="xsdUris">A collection of XSD URI paths to load.</param>
    public XmlSchemaValidatorEx(IEnumerable<string> xsdUris)
        : this()
    {
        foreach (var xsdUri in xsdUris)
        {
            var xDocument = XDocument.Load(xsdUri);
            var xsdNs = (XNamespace)"http://www.w3.org/2001/XMLSchema";

            var targetNamespace = xDocument?.Element(xsdNs + "schema")?.Attribute("targetNamespace") switch
            {
                null => null,
                XAttribute attribute => (string)attribute
            };

            if (targetNamespace != null)
                XmlSchemaSet.Add(targetNamespace, xsdUri);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidatorEx"/> class with multiple schemas loaded from XContainers.
    /// Target namespaces are automatically extracted from each schema document.
    /// </summary>
    /// <param name="xsdContainers">A collection of XContainer objects containing XSD schemas to load.</param>
    public XmlSchemaValidatorEx(IEnumerable<XContainer> xsdContainers)
        : this()
    {
        foreach (var xsdContainer in xsdContainers)
        {
            if (xsdContainer == null) continue;

            var xsdNs = (XNamespace)"http://www.w3.org/2001/XMLSchema";

            var targetNamespace = xsdContainer.Element(xsdNs + "schema")?.Attribute("targetNamespace") switch
            {
                null => null,
                XAttribute attribute => (string)attribute
            };

            if (targetNamespace != null)
                XmlSchemaSet.Add(targetNamespace, xsdContainer.CreateReader());
        }
    }

    /// <summary>
    /// Validates the specified XML document against the loaded schemas.
    /// </summary>
    /// <param name="xDocument">The XML document to validate.</param>
    /// <returns>True if the document is valid (contains no schema errors); otherwise, false. Warnings do not affect validity.</returns>
    public bool IsValid(XDocument xDocument)
    {
        var result = true;
        xDocument.Validate(XmlSchemaSet, (sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
                result = false;
        }, false);

        return result;
    }

    /// <summary>
    /// Gets all schema validation errors for the specified XML document.
    /// </summary>
    /// <param name="xDocument">The XML document to validate.</param>
    /// <returns>A read-only collection of error messages. Returns an empty collection if the document has no errors.</returns>
    public IEnumerable<string> GetErrors(XDocument xDocument)
    {
        var result = new List<string>();
        xDocument.Validate(XmlSchemaSet, (sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
                result.Add(e.Message);
        }, false);

        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets all schema validation warnings for the specified XML document.
    /// </summary>
    /// <param name="xDocument">The XML document to validate.</param>
    /// <returns>A read-only collection of warning messages. Returns an empty collection if the document has no warnings.</returns>
    public IEnumerable<string> GetWarnings(XDocument xDocument)
    {
        var result = new List<string>();
        xDocument.Validate(XmlSchemaSet, (sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Warning)
                result.Add(e.Message);
        }, false);

        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets all schema validation results (both errors and warnings) for the specified XML document.
    /// </summary>
    /// <param name="xDocument">The XML document to validate.</param>
    /// <returns>A read-only collection of validation results containing messages, severity levels, and exceptions. Returns an empty collection if the document has no validation issues.</returns>
    public IEnumerable<XmlValidationResult> GetResults(XDocument xDocument)
    {
        var result = new List<XmlValidationResult>();
        xDocument.Validate(XmlSchemaSet, (sender, e) => result.Add(new XmlValidationResult
        {
            Exception = e.Exception,
            Message = e.Message,
            Severity = e.Severity,
        }), false);

        return result.AsReadOnly();
    }
}
