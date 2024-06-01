using System;

namespace OoBDev.System.MetaData;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MediaTypeAttribute(string mediaType) : Attribute
{
    public string MediaType { get; } = mediaType;
}
