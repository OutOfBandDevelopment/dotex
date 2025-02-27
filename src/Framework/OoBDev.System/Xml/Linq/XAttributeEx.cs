using System;
using System.Xml.Linq;

namespace OoBDev.System.Xml.Linq;

public static class XAttributeEx
{
    public static TEnum AsEnum<TEnum>(this XAttribute xAttribute)
        where TEnum : struct => xAttribute != null && Enum.TryParse<TEnum>((string)xAttribute, out var value) ? value : default;
}
