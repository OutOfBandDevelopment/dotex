using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace OoBDev.System.Reflection;

public static class ObjectExtensions
{
    public static T? As<T>(this object? value) => value == null ? default : (T?)value.As(typeof(T));

    public static object? As(this object? value, Type targetType)
    {
        if (value is null) return default;
        if (targetType.IsInstanceOfType(value)) return value;

        var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is string stringValue)
        {
            if (nonNullableType.IsAssignableTo(typeof(byte[])))
                return Convert.FromBase64String(stringValue);

            var parsableType = nonNullableType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IParsable<>));
            if (parsableType != null)
            {
                object?[] parseArgs = [stringValue, CultureInfo.CurrentCulture, null];
                var parableMethod = parsableType.GetMethod(nameof(IParsable<>.TryParse));
                parableMethod = nonNullableType.GetMethod(parableMethod.Name, [.. parableMethod.GetParameters().Select(p => p.ParameterType)]);
                var parseResult = (bool)parableMethod.Invoke(null, parseArgs);
                if (parseResult)
                    return parseArgs[2];
            }

            var numberType = nonNullableType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>));
            if (numberType != null)
            {
                if (double.TryParse(stringValue, CultureInfo.CurrentCulture, out var doubleValue))
                    return Convert.ChangeType(doubleValue, nonNullableType);
            }
        }

        if (value is byte[] bValue && targetType == typeof(string))
            return Convert.ToBase64String(bValue);

        try
        {
            var converter = TypeDescriptor.GetConverter(nonNullableType);
            if (converter.CanConvertFrom(value.GetType()))
                return converter.ConvertFrom(value)!;

            if (value is string s)
            {
                s = s.Trim('"');

                if (converter.CanConvertFrom(typeof(string)))
                    return converter.ConvertFromString(s)!;

                if (targetType == typeof(byte[]))
                    return Convert.FromBase64String(s);
            }

            converter = TypeDescriptor.GetConverter(value.GetType());
            if (converter.CanConvertTo(nonNullableType))
                return converter.ConvertTo(value, nonNullableType)!;

            return Convert.ChangeType(value, nonNullableType);
        }
        catch
        {
            return default;
        }
    }
}
