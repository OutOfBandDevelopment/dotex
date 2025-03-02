using OoBDev.System.Text;
using System;

namespace OoBDev.System.ComponentModel;

public class ObjectConverter : IObjectConverter
{
    private readonly ISerializer _serializer;

    public ObjectConverter(
        ISerializer serializer
        )
    {
        _serializer = serializer;
    }

    public T? Convert<T>(object? input) where T : class => (T?)Convert(input, typeof(T));

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
