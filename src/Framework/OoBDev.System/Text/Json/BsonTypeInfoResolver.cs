using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace OoBDev.System.Text.Json;

/// <summary>
/// Resolves JSON type information for BSON serialization.
/// </summary>
public class BsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    private static readonly string[] _defaultIdAttributeNames = ["BsonRepresentationAttribute"];

    private readonly HashSet<string> _idAttributeNames;

    /// <summary>
    /// Initializes a resolver that treats <see cref="KeyAttribute"/> and <c>BsonRepresentationAttribute</c> as identifier markers.
    /// </summary>
    public BsonTypeInfoResolver() : this(_defaultIdAttributeNames)
    {
    }

    /// <summary>
    /// Initializes a resolver with its own identifier attributes (matched by type name, so no BSON package is referenced).
    /// </summary>
    /// <param name="idAttributeNames">Attribute type names, besides <see cref="KeyAttribute"/>, that mark a property as the identifier.</param>
    public BsonTypeInfoResolver(IEnumerable<string> idAttributeNames) =>
        _idAttributeNames = new HashSet<string>(idAttributeNames, StringComparer.Ordinal);

    /// <summary>
    /// Gets the JSON type information for the specified type during BSON serialization.
    /// </summary>
    /// <param name="type">The type for which to get JSON type information.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> used for serialization.</param>
    /// <returns>The JSON type information for the specified type.</returns>
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var info = base.GetTypeInfo(type, options);
        var idConvention =
            (
            type.Name.EndsWith("Model", StringComparison.Ordinal) ? type.Name[..^5] :
            type.Name.EndsWith("Collection", StringComparison.Ordinal) ? type.Name[..^10] :
            type.Name
            ) + "Id";

        foreach (var prop in info.Properties)
        {
            var hasIdAttribute = prop.AttributeProvider?.GetCustomAttributes(true).Any(a =>
                a is KeyAttribute ||
                _idAttributeNames.Contains(a.GetType().Name)
                ) ?? false;

            if (string.Equals(prop.Name, idConvention, StringComparison.OrdinalIgnoreCase))
            {
                prop.Name = "_id";
                prop.CustomConverter = new BsonIdConverter();
            }
            else if (hasIdAttribute)
            {
                prop.CustomConverter = new BsonIdConverter();
            }
            else if (new[] {
                typeof(DateTime),
                typeof(DateTime?),
                typeof(DateTimeOffset),
                typeof(DateTimeOffset?),
            }.Contains(prop.PropertyType))
            {
                prop.CustomConverter = new BsonDateTimeOffsetConverter();
            }
        }

        return info;
    }
}
