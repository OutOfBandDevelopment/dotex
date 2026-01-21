using OoBDev.System.Text;
using System;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Provides object conversion functionality using serialization/deserialization.
/// </summary>
public class ObjectConverter : IObjectConverter
{
    private readonly ISerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectConverter"/> class.
    /// </summary>
    /// <param name="serializer">The serializer to use for conversion.</param>
    public ObjectConverter(
        ISerializer serializer
        )
    {
        _serializer = serializer;
    }

    /// <summary>
    /// Converts an object to the specified type using serialization/deserialization.
    /// If the input is already of the target type, it is returned as-is. Otherwise, the input is serialized and deserialized to the target type.
    /// </summary>
    /// <typeparam name="T">The type to convert to (must be a reference type).</typeparam>
    /// <param name="input">The object to convert.</param>
    /// <returns>The converted object, or null if conversion fails.</returns>
    public T? Convert<T>(object? input) where T : class => (T?)Convert(input, typeof(T));

    /// <summary>
    /// Converts an object to the specified type using serialization/deserialization.
    /// If the input is already of the target type, it is returned as-is. String inputs are deserialized directly,
    /// while other types are first serialized to a string and then deserialized to the target type.
    /// </summary>
    /// <param name="input">The object to convert.</param>
    /// <param name="target">The target type to convert to.</param>
    /// <returns>The converted object, or null if conversion fails.</returns>
    //TODO: this is a bit nutty and should be tossed.
    public object? Convert(object? input, Type target) =>
        input switch
        {
            _ when target.IsInstanceOfType(input) => input,
            _ => _serializer.Deserialize(
                input switch
                {
                    string content => content,
                    _ => _serializer.Serialize(input)
                }, target)
        };
}
