using System;

namespace OoBDev.System.Xml.Xsl;

/// <summary>
/// Attribute that marks a method as an XSLT extension function, optionally providing an alternative name for the function.
/// </summary>
/// <param name="name">The name to use for the XSLT function. This name will be used when calling the function from XSLT.</param>
[AttributeUsage(AttributeTargets.Method)]
public class XsltFunctionAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the name of the XSLT function.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets or sets a value indicating whether to hide the original method name when exposing this method as an XSLT function.
    /// When true, only the name specified in the attribute will be available; the original method name will not be exposed.
    /// </summary>
    public bool HideOriginalName { get; set; }
}
