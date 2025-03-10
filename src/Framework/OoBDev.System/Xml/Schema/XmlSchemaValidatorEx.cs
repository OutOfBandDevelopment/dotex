﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace OoBDev.System.Xml.Schema;

public class XmlSchemaValidatorEx
{
    public XmlSchemaSet XmlSchemaSet { get; private set; }

    public XmlSchemaValidatorEx()
    {
        XmlSchemaSet = new XmlSchemaSet();
    }

    public XmlSchemaValidatorEx(string targetNamespace, string xsdUri)
        : this()
    {
        XmlSchemaSet.Add(targetNamespace ?? "", xsdUri);
    }
    public XmlSchemaValidatorEx(string targetNamespace, XmlReader xmlReader)
        : this()
    {
        XmlSchemaSet.Add(targetNamespace ?? "", xmlReader);
    }
    public XmlSchemaValidatorEx(string targetNamespace, XNode xsd)
        : this(targetNamespace ?? "", xsd.CreateReader())
    {
    }
    public XmlSchemaValidatorEx(IEnumerable<KeyValuePair<string, string>> xsdUris)
        : this()
    {
        foreach (var xsdUri in xsdUris.Where(v => v.Value != null))
        {
            XmlSchemaSet.Add(xsdUri.Key ?? "", xsdUri.Value);
        }
    }
    public XmlSchemaValidatorEx(IEnumerable<KeyValuePair<string, XmlReader>> xsdReaders)
        : this()
    {
        foreach (var xsdUri in xsdReaders.Where(v => v.Value != null))
        {
            XmlSchemaSet.Add(xsdUri.Key ?? "", xsdUri.Value);
        }
    }
    public XmlSchemaValidatorEx(IEnumerable<KeyValuePair<string, XNode>> xsds)
        : this()
    {
        foreach (var xsdUri in xsds.Where(v => v.Value != null))
        {
            XmlSchemaSet.Add(xsdUri.Key ?? "", xsdUri.Value.CreateReader());
        }
    }

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
    public IEnumerable<XmlValidationResult> GetResults(XDocument xDocument)
    {
        var result = new List<XmlValidationResult>();
        xDocument.Validate(XmlSchemaSet, (sender, e) =>
        {
            result.Add(new XmlValidationResult
            {
                Exception = e.Exception,
                Message = e.Message,
                Severity = e.Severity,
            });
        }, false);

        return result.AsReadOnly();
    }
}
