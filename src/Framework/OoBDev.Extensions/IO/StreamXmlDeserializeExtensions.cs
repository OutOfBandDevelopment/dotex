using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OoBDev.Extensions.IO;

/// <summary>
/// Set of extension method to centralize deserialize of stream using System.Xml
/// </summary>
public static class StreamXmlDeserializeExtensions
{
    /// <summary>
    /// Convert XML Stream to .Net Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="extraTypes"></param>
    /// <returns></returns>
    public static ValueTask<T?> AsXmlAsync<T>(this Stream? stream, params Type[] extraTypes) =>
        ValueTask.FromResult(stream switch
        {
            null => default,
            _ => (T?)new XmlSerializer(typeof(T), extraTypes).Deserialize(CreateReader(stream))
        });

    /// <summary>
    /// Convert XML Stream to .Net Object
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <param name="extraTypes"></param>
    /// <returns></returns>
    public static ValueTask<object?> AsXmlAsync(this Stream? stream, Type type, params Type[] extraTypes) =>
         ValueTask.FromResult(stream switch
         {
             null => default,
             _ => new XmlSerializer(type, extraTypes).Deserialize(CreateReader(stream))
         });

    /// <summary>
    /// Convert XML Stream to .Net Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="extraTypes"></param>
    /// <returns></returns>
    public static T? AsXml<T>(this Stream? stream, params Type[] extraTypes) =>
        stream switch
        {
            null => default,
            _ => (T?)new XmlSerializer(typeof(T), extraTypes).Deserialize(CreateReader(stream))
        };

    /// <summary>
    /// Convert XML Stream to .Net Object
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <param name="extraTypes"></param>
    /// <returns></returns>
    public static object? AsXml(this Stream? stream, Type type, params Type[] extraTypes) =>
         stream switch
         {
             null => default,
             _ => new XmlSerializer(type, extraTypes).Deserialize(CreateReader(stream))
         };

    // DTD processing is prohibited and no resolver is used so that untrusted documents cannot trigger XXE
    private static XmlReader CreateReader(Stream stream) =>
        XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
}
