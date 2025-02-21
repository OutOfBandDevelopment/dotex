using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.Extensions.IO;

/// <summary>
/// Set of extension method to centralize deserialize of stream using System.Text.Json
/// </summary>
public static class StreamJsonDeserializeExtensions
{
    /// <summary>
    /// Convert JSON Stream to .Net Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async ValueTask<T?> AsJsonAsync<T>(this Stream? stream, JsonSerializerOptions? options = default) =>
        stream switch
        {
            null => default,
            _ => await JsonSerializer.DeserializeAsync<T>(stream, options)
        };

    /// <summary>
    /// Convert JSON Stream to .Net Object
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async ValueTask<object?> AsJsonAsync(this Stream? stream, Type type, JsonSerializerOptions? options = default) =>
        stream switch
        {
            null => default,
            _ => await JsonSerializer.DeserializeAsync(stream, type, options)
        };

    /// <summary>
    /// Convert JSON Stream to .Net Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static T? AsJson<T>(this Stream? stream, JsonSerializerOptions? options = default) =>
        stream switch
        {
            null => default,
            _ => JsonSerializer.Deserialize<T>(stream, options)
        };

    /// <summary>
    /// Convert JSON Stream to .Net Object
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static object? AsJson(this Stream? stream, Type type, JsonSerializerOptions? options = default) =>
        stream switch
        {
            null => default,
            _ => JsonSerializer.Deserialize(stream, type, options)
        };
}
