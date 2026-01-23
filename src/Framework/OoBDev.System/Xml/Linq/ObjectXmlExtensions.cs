using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace OoBDev.System.Xml.Linq;

/// <summary>
/// Provides extension methods for converting objects to XML representations using reflection.
/// Supports conversion of simple types, collections, streams, and complex objects to XElement instances.
/// </summary>
public static class ObjectXmlExtensions
{
    /// <summary>
    /// Converts an object to an XElement using reflection to map properties and values.
    /// Handles XElement and XDocument inputs directly, and uses reflection for other types.
    /// </summary>
    /// <param name="input">The object to convert to XML. Can be null, XElement, XDocument, or any other object.</param>
    /// <returns>An XElement representation of the object, or null if the input is null.</returns>
    public static XElement? AsXElement(this object input) =>
        input switch
        {
            null => null,
            XElement element => element,
            XDocument document => XElement.Load(document.CreateReader()),
            _ => ReflectObjectXml(input)
        };

    private static XObject? ReflectPropertyXml(PropertyInfo prop, object? input, XName parentName)
    {
        var name = XmlConvert.EncodeName(prop.Name);
        var val = prop.GetValue(input, null);
        return val == null || val == DBNull.Value
            ? null
            : prop.PropertyType.IsSimpleType()
            ? new XAttribute(name, val)
            : ReflectObjectXml(val, XName.Get(name, parentName?.NamespaceName ?? ""));
    }

    private static XElement? ReflectObjectXml(object input, XName? elementName = null)
    {
        if (input == null)
            return null;

        if (elementName == null) elementName = input.GetXmlElementName();

        var type = input.GetType();

        var ret = new XElement(elementName);
        if (type.IsSimpleType())
        {
            ret.Add(input);
        }
        else
        {
            if (input is MemoryStream ms)
            {
                input = ms.ToArray();
            }
            if (input is Stream stream)
            {
                using var newMs = new MemoryStream();
                stream.CopyTo(newMs);
                input = newMs.ToArray();
            }

            var enumerable = input as IEnumerable;

            if (enumerable != null)
            {
                if (input is IEnumerable<char> || input is char[])
                {
                    input = new string([.. enumerable.Cast<char>()]);
                }
                else if (input is IEnumerable<byte> || input is byte[])
                {
                    input = Convert.ToBase64String(enumerable.Cast<byte>().ToArray());
                }
            }
            if (input is string) // ensure strings are written as text 
            {
                ret.Add(input);
            }
            else if (enumerable != null)
            {
                var itemName = enumerable.GetXmlItemName(elementName);

                var elements = from item in enumerable.Cast<object>()
                               where item != null
                               let itemType = item.GetType()
                               select ReflectObjectXml(item, itemName);

                ret.Add(elements);
                if (!ret.HasElements)
                    return null;
            }
            else
            {
                var props = type.GetProperties();
                var elements = from prop in props
                               let value = ReflectPropertyXml(prop, input, elementName)
                               where value != null
                               select value;

                ret.Add(elements);
            }
        }

        return ret;
    }
}
