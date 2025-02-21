using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Text;

/// <summary>
/// Interface to identify shared serialization process.  
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// convert the object based on the type definition
    /// </summary>
    /// <param name="obj">object to serialize</param>
    /// <param name="type">template model to serialize</param>
    /// <returns></returns>
    string Serialize(object? obj, Type type);

    /// <summary>
    /// convert the object based on the type definition
    /// </summary>
    /// <typeparam name="T">template model to serialize</typeparam>
    /// <param name="obj">object to serialize</param>
    /// <returns></returns>
    string Serialize<T>(T obj);

    /// <summary>
    /// convert the object based on the type definition
    /// </summary>
    /// <param name="obj">object to serialize</param>
    /// <param name="type">template model to serialize</param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SerializeAsync(object? obj, Type type, Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// convert the object based on the type definition
    /// </summary>
    /// <typeparam name="T">template model to serialize</typeparam>
    /// <param name="obj">object to serialize</param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SerializeAsync<T>(T obj, Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <typeparam name="T">template model to deserialize</typeparam>
    /// <param name="stream"></param>
    /// <returns></returns>
    T? Deserialize<T>(Stream stream);

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type">template model to deserialize</param>
    /// <returns></returns>
    object? Deserialize(Stream stream, Type type);

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <typeparam name="T">template model to deserialize</typeparam>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type">template model to deserialize</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<object?> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default);

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <typeparam name="T">template model to deserialize</typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    T? Deserialize<T>(string input);

    /// <summary>
    /// convert stream into object
    /// </summary>
    /// <param name="input"></param>
    /// <param name="type">template model to deserialize</param>
    /// <returns></returns>
    object? Deserialize(string input, Type type);

    /// <summary>
    /// Content type supported by this serializer
    /// </summary>
    string ContentType { get; }
}
