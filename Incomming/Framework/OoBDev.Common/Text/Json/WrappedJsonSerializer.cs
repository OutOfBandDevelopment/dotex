using System.IO;
using System.Text.Json;

namespace OoBDev.Common.Text.Json;

public class WrappedJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions? _jsonSerializerOptions;

    public WrappedJsonSerializer
        (
        JsonSerializerOptions? jsonSerializerOptions = null
        ) => _jsonSerializerOptions = jsonSerializerOptions;

    public T? Deserialize<T>(string input) =>
        JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);
    public T? Deserialize<T>(Stream stream) =>
        JsonSerializer.Deserialize<T>(stream, _jsonSerializerOptions);

    public string Serialize<T>(T input) =>
        JsonSerializer.Serialize(input, _jsonSerializerOptions);
}
