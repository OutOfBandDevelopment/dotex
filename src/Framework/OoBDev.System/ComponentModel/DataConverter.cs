using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Provides type conversion functionality with support for complex types, JSON values, and custom mappings.
/// </summary>
public class DataConverter : IDataConverter
{
    private readonly Dictionary<Type, Func<object, object>> _mappedConverters = new()
    {
        {typeof(int), v=>Convert.ToInt32(v) },
        {typeof(int?), v=>Convert.ToInt32(v) },
        {typeof(uint), v=>Convert.ToUInt32(v) },
        {typeof(uint?), v=>Convert.ToUInt32(v) },

        {typeof(long), v=>Convert.ToInt64(v) },
        {typeof(long?), v=>Convert.ToInt64(v) },
        {typeof(ulong), v=>Convert.ToUInt64(v) },
        {typeof(ulong?), v=>Convert.ToUInt64(v) },

        {typeof(decimal), v=>Convert.ToDecimal(v) },
        {typeof(decimal?), v=>Convert.ToDecimal(v) },

        {typeof(double), v=>Convert.ToDouble(v) },
        {typeof(double?), v=>Convert.ToDouble(v) },
        {typeof(float), v=>Convert.ToSingle(v) },
        {typeof(float?), v=>Convert.ToSingle(v) },
    };

    /// <summary>
    /// Converts the specified value to the target type using various conversion strategies including TypeDescriptor, JSON deserialization, and custom mappings.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value, or <c>null</c> if the value is null or conversion fails.</returns>
    public T? ConvertTo<T>(object? value) => (T?)ConvertTo(value, typeof(T), default(T));

    /// <summary>
    /// Converts the specified value to the specified type using various conversion strategies including TypeDescriptor, JSON deserialization, custom mappings, and array splitting.
    /// Supports conversion from JSON values, handles TimeSpan with leading '+' sign, and can convert delimited strings to arrays.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="type">The target type to convert to.</param>
    /// <param name="defaultValue">The default value to return if conversion fails (currently not used, method throws on failure).</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotSupportedException">Thrown when the conversion is not supported.</exception>
    public object? ConvertTo(object? value, Type type, object? defaultValue = null)
    {
        if (IsNull(value)) return default;
        else if (type.IsAssignableFrom(value!.GetType())) return value;
        else if (value is JsonValue jsonValue)
        {
            var result = jsonValue.GetValueKind() switch
            {
                JsonValueKind.Number => ConvertTo(jsonValue.GetValue<double>(), type, defaultValue),
                JsonValueKind.String => ConvertTo(jsonValue.GetValue<string>(), type, defaultValue),
                JsonValueKind.Array => ConvertTo(jsonValue.GetValue<object[]>(), type, defaultValue),
                _ => null
            };
            return result;
        }

        if (value is string stringValue && type == typeof(TimeSpan) && stringValue.StartsWith('+'))
        {
            value = stringValue[1..];
        }

        var sourceConverter = TypeDescriptor.GetConverter(value);
        if (sourceConverter?.CanConvertTo(type) ?? false)
        {
            return sourceConverter.ConvertTo(value, type);
        }

        var targetConverter = TypeDescriptor.GetConverter(type);
        if (targetConverter?.CanConvertFrom(value.GetType()) ?? false)
        {
            return targetConverter.ConvertFrom(value);
        }

        if (_mappedConverters.TryGetValue(type, out var mappedConverter))
            return mappedConverter(value);

        if (type.IsArray && value is string splitableString)
        {
            var elementType = type.GetElementType();
            if (elementType is not null)
            {
                var converted = Split(value)
                    ?.Select(i => ConvertTo(i, elementType, null))
                    .Where(i => i != null)
                    .ToArray();

                if (converted is not null)
                {
                    var targetArray = Array.CreateInstance(elementType, converted.Length);

                    Array.Copy(converted, targetArray, converted.Length);

                    if (targetArray.Length > 0)
                    {
                        return targetArray;
                    }
                }
            }
        }

        throw new NotSupportedException($"Unable to convert \"{value}\" ({value.GetType()}) to {type}");
    }

    /// <summary>
    /// Determines whether the specified value is considered null, including support for DBNull, empty/whitespace strings,
    /// JSON null values, empty collections, and empty arrays.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> if the value is null, DBNull, empty string, empty collection, or empty array; otherwise, <c>false</c>.</returns>
    public bool IsNull(object? value)
    {
        if (value is null || value is DBNull || value == DBNull.Value) return true;
        else if (value is string stringvalue && string.IsNullOrWhiteSpace(stringvalue)) return true;
        else if (value is JsonValue jsonValue && jsonValue.GetValueKind() == JsonValueKind.Null) return true;
        else if (value is JsonArray jsonArray && jsonArray.Count == 0) return true;
        else if (value is ICollection collection && collection.Count == 0) return true;
        else if (value is Array array && array.Length == 0) return true;
        return false;
    }

    /// <summary>
    /// Splits a value into an array of objects, supporting arrays, enumerables, JSON arrays, and comma-delimited strings.
    /// Attempts JSON parsing for string values and handles nested JSON structures.
    /// </summary>
    /// <param name="value">The value to split (can be an array, enumerable, JSON string, or delimited string).</param>
    /// <returns>An array of objects, or null if the value is null. Non-null elements are filtered from the result.</returns>
    public object[]? Split(object? value)
    {
        if (IsNull(value)) return default;
        else if (value is Array arrayvalue)
        {
            return [.. arrayvalue.Cast<object>().Where(i => !IsNull(i))];
        }
        else if (value is not string and IEnumerable enumerableValue)
        {
            return [.. enumerableValue.Cast<object>().Where(i => !IsNull(i))];
        }

        if (value is string stringValue)
        {
            try
            {
                var jsonString =
                    stringValue.Trim().StartsWith('[') ? stringValue : ('[' + stringValue + ']');

                //Note: this is json abuse to improve LLM support.
                if (jsonString.Contains('\''))
                    jsonString = jsonString.Replace("\"", "\\\"").Replace("'", "\"");

                var innerJson = JsonNode.Parse(jsonString);
                var nested = Split(innerJson);
                return nested;
            }
            catch (JsonException)
            {
                //Note: don't care
            }

            return [.. stringValue.Split(',').Cast<object>().Where(i => !IsNull(i))];
        }
        else if (value is JsonNode jsonNode)
        {
            if (jsonNode is JsonArray jsonArray)
            {
                return [.. jsonArray.OfType<object>().Where(i => !IsNull(i))];
            }
            else if (jsonNode is JsonValue jsonValue && jsonValue.GetValueKind() == JsonValueKind.String)
            {
                //TODO: try parse
            }
        }

        return [value!];
    }
}
