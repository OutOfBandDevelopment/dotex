using System.IO;

namespace OoBDev.Common.Text.Json;

/// <summary>
/// Defines methods for serializing and deserializing objects to and from JSON format.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Deserializes a JSON string into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="input">The JSON string to deserialize.</param>
    /// <returns>An object of type <typeparamref name="T"/>, or <c>null</c> if deserialization fails.</returns>
    public T? Deserialize<T>(string input);

    /// <summary>
    /// Deserializes a JSON stream into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="stream">The stream containing JSON data to deserialize.</param>
    /// <returns>An object of type <typeparamref name="T"/>, or <c>null</c> if deserialization fails.</returns>

    public T? Deserialize<T>(Stream stream);

    /// <summary>
    /// Serializes an object of the specified type into a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public string Serialize<T>(T input);
}
