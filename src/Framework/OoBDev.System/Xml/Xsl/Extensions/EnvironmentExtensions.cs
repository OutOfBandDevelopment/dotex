﻿using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using static OoBDev.System.ToolkitConstants;

namespace OoBDev.System.Xml.Xsl.Extensions;

/// <summary>
/// This is a wrapper around <c>System.Environment</c> intended for use with XslCompiledTransform
/// </summary>
[XmlRoot(Namespace = XmlNamespaces.Base + nameof(EnvironmentExtensions))]
public class EnvironmentExtensions
{
    private readonly XNamespace _ns;

    public EnvironmentExtensions()
    {
        _ns = this.GetXmlNamespace() + XmlNamespaces.OutputSuffix;
    }

    /// <summary>
    /// Retrieves the value of an environment variable from the current process.
    /// </summary>
    /// <param name="variableName">The name of the environment variable.</param>
    /// <returns>The value of the environment variable specified by variable, or null if the environment variable is not found.</returns>
    public string GetValue(string variableName) =>
        Environment.GetEnvironmentVariable(variableName) ?? "";

    /// <summary>
    /// Retrieves all environment variable names and their values from the current process.
    /// </summary>
    /// <returns>XML element with an attribute for each current environment variable</returns>
    public XPathNavigator? GetValues()
    {
        var xml = new XElement(_ns + "variables",
             from k in Environment.GetEnvironmentVariables().Keys.Cast<string>()
             select new XAttribute(XmlConvert.EncodeLocalName(k), GetValue(k))
             );
        return xml.ToXPathNavigable().CreateNavigator();
    }
}
