using System;
using System.Xml.Linq;

namespace OoBDev.System.Xml.Linq;

/// <summary>
/// Provides extension methods for XAttribute operations.
/// </summary>
public static class XAttributeEx
{
    /// <summary>
    /// Converts an XAttribute value to an enum type.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to convert to.</typeparam>
    /// <param name="xAttribute">The XML attribute to convert.</param>
    /// <returns>The enum value if parsing succeeds, otherwise the default value of the enum type.</returns>
    public static TEnum AsEnum<TEnum>(this XAttribute xAttribute)
        where TEnum : struct => xAttribute != null && Enum.TryParse<TEnum>((string)xAttribute, out var value) ? value : default;
}
