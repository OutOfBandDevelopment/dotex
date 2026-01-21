using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace OoBDev.System.Xml.Linq;

/// <summary>
/// Provides extension methods for XElement operations.
/// </summary>
public static class XElementEx
{
    /// <summary>
    /// Converts an XElement to an XmlNode.
    /// </summary>
    /// <param name="element">The XElement to convert.</param>
    /// <returns>The converted XmlNode, or null if the element is null.</returns>
    public static XmlNode? ToXmlNode(this XElement element)
    {
        using var xmlReader = element.CreateReader();
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlReader);
        return xmlDoc.FirstChild;
    }

    /// <summary>
    /// Gets the first descendant element with the specified name and returns its value as a string.
    /// </summary>
    /// <param name="element">The parent element to search within.</param>
    /// <param name="name">The name of the descendant element to find.</param>
    /// <returns>The string value of the first matching descendant, or null if not found.</returns>
    public static string? GetDescendantAsString(this XElement element, XName name) =>
        (string?)element.Descendants(name).FirstOrDefault();

    /// <summary>
    /// Gets the first descendant element with the specified name and returns its value as a long.
    /// </summary>
    /// <param name="element">The parent element to search within.</param>
    /// <param name="name">The name of the descendant element to find.</param>
    /// <returns>The long value of the first matching descendant, or null if not found or conversion fails.</returns>
    public static long? GetDescendantAsLong(this XElement element, XName name) =>
        (long?)element?.Descendants(name).FirstOrDefault();

    /// <summary>
    /// Gets the value attribute of the first element in the collection with a matching name attribute.
    /// </summary>
    /// <param name="elements">The collection of elements to search.</param>
    /// <param name="name">The name attribute value to match (case-insensitive).</param>
    /// <returns>The value attribute of the matching element, or null if not found.</returns>
    public static string? GetTargetValue(this IEnumerable<XElement> elements, string name) =>
        elements?.Where(e => string.Equals((string?)e.Attribute("name"), name, StringComparison.InvariantCultureIgnoreCase))
                       .Select(e => (string?)e.Attribute("value"))
                       .FirstOrDefault();
}
