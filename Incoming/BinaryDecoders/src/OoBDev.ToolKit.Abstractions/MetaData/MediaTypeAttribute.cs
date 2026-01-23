using System;

namespace OoBDev.ToolKit.MetaData;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MediaTypeAttribute(string mediaType) : Attribute
{
    public string MediaType { get; } = mediaType;
}
