using OoBDev.System.Xml.XPath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace OoBDev.System.Reflection;

/// <summary>
/// Builds an XML node tree from a .NET object using reflection.
/// Supports customization of namespace handling, type details, and navigation rules.
/// </summary>
/// <param name="seed">The root object to build the XML node tree from.</param>
/// <param name="excludeNamespace">If true, excludes XML namespace information from the generated nodes.</param>
/// <param name="excludeTypeDetails">If true, excludes type metadata attributes from the generated nodes.</param>
public class ReflectionElementNodeBuilder(object seed, bool excludeNamespace = false, bool excludeTypeDetails = false)
{
    /// <summary>
    /// Gets a value indicating whether XML namespaces should be excluded from generated nodes.
    /// </summary>
    protected bool ExcludeNamespace { get; } = excludeNamespace;

    /// <summary>
    /// Gets a value indicating whether type details should be excluded from generated nodes.
    /// </summary>
    protected bool ExcludeTypeDetails { get; } = excludeTypeDetails;

    /// <summary>
    /// Gets the root object being converted to an XML node tree.
    /// </summary>
    protected object Seed { get; } = seed;

    /// <summary>
    /// Builds an INode tree from the seed object using reflection.
    /// </summary>
    /// <returns>An INode representing the root of the XML node tree.</returns>
    public INode Build() =>
        new ExtensibleElementNode(
             Seed.GetXmlElementName(ExcludeNamespace),
             Seed,
             ValueSelector,
             AttributeSelector,
             ChildSelector,
             NamespacesSelector,
             PreserveWhitespace
             );

    /// <summary>
    /// Determines whether whitespace should be preserved for the given object.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True to preserve whitespace; otherwise, false. Default is true.</returns>
    protected virtual bool PreserveWhitespace(object obj) => true;

    /// <summary>
    /// Selects the XML namespaces to include for the given model object.
    /// </summary>
    /// <param name="model">The model object.</param>
    /// <returns>A collection of XML namespace names, or null.</returns>
    protected virtual IEnumerable<XName>? NamespacesSelector(object model) => [];

    /// <summary>
    /// Selects the child nodes for the given model object.
    /// For value types, returns null. For IEnumerable types, returns enumerable items.
    /// For other objects, returns properties that can be navigated.
    /// </summary>
    /// <param name="model">The model object to get children from.</param>
    /// <returns>A collection of child name/object tuples, or null if the model is a value type.</returns>
    protected virtual IEnumerable<(XName name, object child)>? ChildSelector(object model) =>
         IsValue(model) ? null : model switch
         {
             null => null,
             IEnumerable enumerable => from item in enumerable.Cast<object>()
                                       where item != null
                                       select (item.GetXmlElementName(ExcludeNamespace), item),
             _ => from property in model.GetType().GetProperties() ?? Enumerable.Empty<PropertyInfo>()
                  where property.CanRead && AllowNavigate(model, property)
                  select (XName.Get(property.Name, ExcludeNamespace ? "" : model.GetXmlNamespace()), SafeRead(model, property))
         };

    /// <summary>
    /// Determines whether navigation to the specified property is allowed for the given model object.
    /// Excludes indexed properties and properties on Type objects.
    /// </summary>
    /// <param name="model">The model object containing the property.</param>
    /// <param name="property">The property to check.</param>
    /// <returns>True if navigation is allowed; otherwise, false.</returns>
    protected virtual bool AllowNavigate(object model, PropertyInfo property) =>
        model switch
        {
            null => false,
            Type _ => false,
            _ => property.GetIndexParameters() switch
            {
                ParameterInfo[] indexes when indexes.Length > 0 => false,
                _ => true,
            }
        };

    /// <summary>
    /// Safely reads a property value from the model object, catching and logging any exceptions.
    /// </summary>
    /// <param name="model">The model object to read the property from.</param>
    /// <param name="property">The property to read.</param>
    /// <returns>The property value, or null if an exception occurs.</returns>
    protected virtual object? SafeRead(object model, PropertyInfo property)
    {
        try
        {
            return property.GetValue(model);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to read {property.Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Selects the XML attributes to include for the given model object.
    /// If ExcludeTypeDetails is false, includes type metadata (AssemblyQualifiedName, Name, FullName, Namespace).
    /// </summary>
    /// <param name="model">The model object to get attributes from.</param>
    /// <returns>A collection of attribute name/value tuples, or null if type details are excluded or model is null.</returns>
    protected virtual IEnumerable<(XName name, string? value)>? AttributeSelector(object model) =>
        ExcludeTypeDetails switch
        {
            true => null,
            false => model switch
            {
                null => null,
                // Type type => new (XName name, string? value)[] {
                //    (XName.Get("AssemblyQualifiedName"), type.AssemblyQualifiedName),
                //    (XName.Get("Name"), type.Name),
                //    (XName.Get("FullName"),type.FullName),
                //    (XName.Get("Namespace"), type.Namespace),
                //},
                _ => new (XName name, string? value)[] {
                   (XName.Get("AssemblyQualifiedName"), model?.GetType()?.AssemblyQualifiedName),
                   (XName.Get("Name"), model?.GetType()?.Name),
                   (XName.Get("FullName"), model?.GetType()?.FullName),
                   (XName.Get("Namespace"), model?.GetType()?.Namespace),
               }
            }
        };

    /// <summary>
    /// Selects the string value representation for the given model object.
    /// Handles special types like Type (returns AssemblyQualifiedName), byte arrays (Base64),
    /// streams (Base64), and character arrays/sequences (converts to string).
    /// </summary>
    /// <param name="model">The model object to get the value from.</param>
    /// <returns>A string representation of the value, or null if not a value type.</returns>
    protected virtual string? ValueSelector(object? model) =>
        IsValue(model) ? model switch
        {
            null => null,

            Type type => type.AssemblyQualifiedName,

            byte[] bytes => Convert.ToBase64String(bytes),
            IEnumerable<byte> bytes => Convert.ToBase64String(bytes.ToArray()),
            MemoryStream ms => Convert.ToBase64String(ms.ToArray()),
            Stream stream => ValueSelector(stream.AsBytes()),

            string @string => @string,
            char[] chars => new string(chars),
            IEnumerable<char> chars => new string([.. chars]),

            _ => null // model.ToString()
        } : null;

    /// <summary>
    /// Determines whether the given object should be treated as a value (leaf node) rather than a complex object.
    /// Returns true for simple types, byte arrays, character arrays, and streams.
    /// </summary>
    /// <param name="input">The object to check.</param>
    /// <returns>True if the object is a value type; otherwise, false.</returns>
    protected virtual bool IsValue(object? input) =>
        input switch
        {
            null => false,
            _ when input.GetType().IsSimpleType() => true,
            byte[] _ => true,
            IEnumerable<byte> _ => true,
            char[] _ => true,
            IEnumerable<char> _ => true,
            Stream _ => true,
            _ => false
        };
}
