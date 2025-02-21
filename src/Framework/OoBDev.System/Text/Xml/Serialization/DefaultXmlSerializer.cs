using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OoBDev.System.Text.Xml.Serialization;

/// <summary>
/// Default XmlSerializer, 
/// </summary>
public class DefaultXmlSerializer : IXmlSerializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultXmlSerializer"/> class.
    /// </summary>
    // <param name="options">JSON serialization options (optional).</param>
    public DefaultXmlSerializer(
    // JsonSerializerOptions? options = null
    )
    {
        //_options = options ?? DefaultOptions;
    }

    /// <summary>
    /// Gets the default content type for XML serialization.
    /// </summary>
    public const string DefaultContentType = "text/xml";

    /// <summary>
    /// Gets the content type used for XML serialization.
    /// </summary>
    public virtual string ContentType => DefaultContentType;

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <typeparam name="T">template model to deserialize</typeparam>
    /// <param name="stream"></param>
    /// <returns></returns>
    public virtual T? Deserialize<T>(Stream stream) =>
        (T?)new XmlSerializer(typeof(T)).Deserialize(stream);

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type">template model to deserialize</param>
    /// <returns></returns>
    public virtual object? Deserialize(Stream stream, Type type) =>
        new XmlSerializer(type).Deserialize(stream);

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <typeparam name="T">template model to deserialize</typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public virtual T? Deserialize<T>(string input)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms) { AutoFlush = true };
        writer.Write(input);
        ms.Position = 0;
        return Deserialize<T>(ms);
    }

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <param name="input"></param>
    /// <param name="type">template model to deserialize</param>
    /// <returns></returns>
    public virtual object? Deserialize(string input, Type type)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms) { AutoFlush = true };
        writer.Write(input);
        ms.Position = 0;
        return Deserialize(ms, type);
    }

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <typeparam name="T">template model to deserialize</typeparam>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default) =>
        new(Deserialize<T>(stream));

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type">template model to deserialize</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual ValueTask<object?> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default) =>
        new(Deserialize(stream, type));

    /// <summary>
    /// convert the object based on the type definition
    /// </summary>
    /// <param name="obj">object to serialize</param>
    /// <param name="type">template model to serialize</param>
    /// <returns></returns>
    public virtual string Serialize(object? obj, Type type)
    {
        var xml = new XmlSerializer(type);

        using var ms = new MemoryStream();
        using var writer = XmlWriter.Create(ms);
        xml.Serialize(writer, obj);
        ms.Position = 0;
        using var reader = new StreamReader(ms);
        var read = reader.ReadToEnd();
        return read;
    }

    /// <summary>
    /// convert the object based on the type definition
    /// </summary>
    /// <typeparam name="T">template model to serialize</typeparam>
    /// <param name="obj">object to serialize</param>
    /// <returns></returns>
    public virtual string Serialize<T>(T obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        var xml = new XmlSerializer(obj.GetType());

        using var ms = new MemoryStream();
        using var writer = XmlWriter.Create(ms);
        xml.Serialize(writer, obj);
        ms.Position = 0;
        using var reader = new StreamReader(ms);
        var read = reader.ReadToEnd();
        return read;
    }

    /// <summary>
    /// convert the object based on the type definition
    /// </summary>
    /// <param name="obj">object to serialize</param>
    /// <param name="type">template model to serialize</param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task SerializeAsync(object? obj, Type type, Stream stream, CancellationToken cancellationToken = default)
    {
        var xml = new XmlSerializer(type);
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            CloseOutput = false,
        });
        xml.Serialize(writer, obj);
        await writer.FlushAsync();
    }

    /// <summary>
    /// convert the object based on the type definition
    /// </summary>
    /// <param name="obj">object to serialize</param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task SerializeAsync<T>(T obj, Stream stream, CancellationToken cancellationToken = default)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        var xml = new XmlSerializer(obj.GetType());
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            CloseOutput = false,
        });
        xml.Serialize(writer, obj);
        await writer.FlushAsync();
    }
}
